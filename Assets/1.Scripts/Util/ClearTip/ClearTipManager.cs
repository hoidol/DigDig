using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

// (2026-11-26) 스테이지 클리어 팁 (Realtime Database, 랭킹 인스턴스 FirebaseDatabaseConfig.RANKING_URL)
//
// 경로
//   clearTips/{스테이지 key}/{uid} = { nickname, text, updatedAt, pinned? }
//     스테이지 key = StageData.MakeKey(order, 난이도) (예 Normal_0 = 스테이지 1 보통)
//     유저당 스테이지·난이도마다 1개 (다시 남기면 덮어씀)
//     pinned = true 인 항목은 항상 맨 앞에 (운영자가 콘솔에서 직접 넣는 팁: uid 자리에 아무 key, 예 "admin_1")
//   clearTipConfig/bannedWords = "단어1,단어2,…" (쉼표 구분 문자열, 콘솔에서 수정) → 코드 기본 목록과 합쳐 *** 로 가림
//
// 정책
// - 쓰기: 그 스테이지·난이도를 클리어한 유저만 (UserStage.isClear), 공백 정리 후 MAX_LENGTH(40)자, 금칙어는 올릴 때도 *** (화면에서도 다시 가림 → 나중에 추가한 금칙어도 적용)
// - 읽기: updatedAt 기준 최신 FETCH_COUNT(30)개 + pinned. 스테이지 key별 REFRESH_INTERVAL_SEC(5분) 캐시, 내가 올리면 그 key 캐시는 버림
// - 금칙어 목록은 한 번 읽으면 앱을 켠 동안 유지
// - (2026-11-26) 내가 이번에 남긴 팁은 띠지 2번째 줄 (결과 화면 → 로비로 돌아왔을 때 바로 보이게, PlaceMine. 앱을 켠 동안)
// - 에디터엔 Firebase가 없음 → 테스트는 FetchForTest / UploadForTest (기본 null)
public static class ClearTipManager
{
    public const int MAX_LENGTH = 40;
    public const int FETCH_COUNT = 30;
    public const float REFRESH_INTERVAL_SEC = 300f;

    const string TIPS_PATH = "clearTips";
    const string CONFIG_PATH = "clearTipConfig";
    const string BANNED_KEY = "bannedWords";
    const string NICKNAME_KEY = "nickname";
    const string TEXT_KEY = "text";
    const string UPDATED_AT_KEY = "updatedAt";
    const string PINNED_KEY = "pinned";

    // 서버 목록이 비었을 때도 쓰는 기본 금칙어 (서버 bannedWords와 합침)
    static readonly string[] DEFAULT_BANNED_WORDS = { "시발", "씨발", "ㅅㅂ", "병신", "ㅂㅅ", "좆", "개새끼", "fuck", "shit" };

    class Cache
    {
        public List<ClearTip> tips;
        public float fetchedAt;
    }

    static readonly Dictionary<string, Cache> caches = new Dictionary<string, Cache>();
    static List<string> bannedWords;

    // (2026-11-26) 이번에 앱을 켠 동안 내가 남긴 팁 (스테이지 key → 팁). 로비 띠지에서 2번째 줄에 보이게 (PlaceMine)
    static readonly Dictionary<string, ClearTip> myPosted = new Dictionary<string, ClearTip>();
    public const int MY_TIP_INDEX = 1; // 0부터 → 2번째

    // 테스트용 (에디터엔 Firebase 네이티브가 없음)
    public static Func<string, List<ClearTip>> FetchForTest;
    public static Func<string, ClearTip, bool> UploadForTest;
    public static bool IsTestMode => FetchForTest != null || UploadForTest != null;

    public static string CurrentStageKey => StageData.MakeKey(UserDataManager.stageOrder, UserDataManager.difficultyType);

    // ---- 규칙 (씬 없이 계산)
    // 공백 정리 + 길이 제한
    public static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";
        string one = string.Join(" ", text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries));
        return one.Length > MAX_LENGTH ? one.Substring(0, MAX_LENGTH) : one;
    }

    // 금칙어를 같은 길이 * 로 (대소문자 무시)
    public static string Mask(string text, IEnumerable<string> words)
    {
        if (string.IsNullOrEmpty(text) || words == null)
            return text;
        var sb = new StringBuilder(text);
        string lower = text.ToLowerInvariant();
        foreach (string w in words)
        {
            if (string.IsNullOrWhiteSpace(w))
                continue;
            string word = w.Trim().ToLowerInvariant();
            int idx = 0;
            while ((idx = lower.IndexOf(word, idx, StringComparison.Ordinal)) >= 0)
            {
                for (int i = 0; i < word.Length; i++)
                    sb[idx + i] = '*';
                idx += word.Length;
            }
        }
        return sb.ToString();
    }

    public static List<string> ParseBannedWords(string raw) => string.IsNullOrEmpty(raw) ? new List<string>()
        : raw.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim()).Where(w => w.Length > 0).ToList();

    public static IEnumerable<string> BannedWords => (bannedWords ?? new List<string>()).Concat(DEFAULT_BANNED_WORDS);

    public static string MaskNow(string text) => Mask(text, BannedWords);

    // 띠지 순서: pinned 먼저, 그다음 최신순
    public static List<ClearTip> Order(IEnumerable<ClearTip> tips) =>
        tips.Where(t => t != null && !string.IsNullOrWhiteSpace(t.text))
            .OrderByDescending(t => t.pinned).ThenByDescending(t => t.updatedAt).ToList();

    // 내 팁을 MY_TIP_INDEX(2번째) 자리로 (같은 uid의 서버 항목은 빼고, 목록이 짧으면 맨 끝). mine이 없으면 그대로
    public static List<ClearTip> PlaceMine(List<ClearTip> ordered, ClearTip mine)
    {
        if (mine == null)
            return ordered;
        var list = ordered.Where(t => t.uid != mine.uid).ToList();
        list.Insert(Mathf.Min(MY_TIP_INDEX, list.Count), mine);
        return list;
    }

    static List<ClearTip> WithMine(string stageKey, List<ClearTip> tips) =>
        PlaceMine(tips, myPosted.TryGetValue(stageKey, out ClearTip mine) ? MaskTip(mine) : null);

    // 이 스테이지·난이도를 클리어했는지 (팁을 남길 자격)
    public static bool CanWrite(string stageKey)
    {
        if (UserDataManager.Instance == null || !UserStageManager.TryParseKey(stageKey, out int order, out DifficultyType difficulty))
            return false;
        UserStage stage = UserDataManager.Instance.userStageManager.FindUserStage(order, difficulty);
        return stage != null && stage.isClear;
    }

    // ---- 서버
    // 최신 팁 (캐시 5분). 실패하면 이전 캐시 또는 빈 목록
    public static async UniTask<List<ClearTip>> GetTipsAsync(string stageKey)
    {
        if (FetchForTest != null)
            return WithMine(stageKey, Order(FetchForTest(stageKey)).Select(MaskTip).ToList());
        if (caches.TryGetValue(stageKey, out Cache cache) && Time.realtimeSinceStartup - cache.fetchedAt < REFRESH_INTERVAL_SEC)
            return WithMine(stageKey, cache.tips);
        try
        {
            if (FirebaseManager.Instance == null || FirebaseManager.Instance.firebaseAuth == null)
                return WithMine(stageKey, cache?.tips ?? new List<ClearTip>());
            string uid = await FirebaseManager.Instance.firebaseAuth.EnsureSignedInAsync();
            if (string.IsNullOrEmpty(uid))
                return WithMine(stageKey, cache?.tips ?? new List<ClearTip>());
            await LoadBannedWordsAsync();

            var tipsRef = FirebaseDatabaseConfig.Ranking.RootReference.Child(TIPS_PATH).Child(stageKey);
            var latest = await tipsRef.OrderByChild(UPDATED_AT_KEY).LimitToLast(FETCH_COUNT).GetValueAsync().AsUniTask();
            var pinned = await tipsRef.OrderByChild(PINNED_KEY).EqualTo(true).GetValueAsync().AsUniTask();
            var dic = new Dictionary<string, ClearTip>();
            foreach (var snap in new[] { latest, pinned })
                foreach (var child in snap.Children)
                    dic[child.Key] = new ClearTip
                    {
                        uid = child.Key,
                        nickname = child.Child(NICKNAME_KEY).Value as string ?? "",
                        text = child.Child(TEXT_KEY).Value as string ?? "",
                        updatedAt = ToLong(child.Child(UPDATED_AT_KEY).Value),
                        pinned = child.Child(PINNED_KEY).Value is bool b && b,
                    };
            List<ClearTip> tips = Order(dic.Values).Select(MaskTip).ToList();
            caches[stageKey] = new Cache { tips = tips, fetchedAt = Time.realtimeSinceStartup };
            Debug.Log($"[ClearTipManager] 팁 불러오기 ({stageKey}) {tips.Count}개");
            return WithMine(stageKey, tips);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[ClearTipManager] 팁 불러오기 실패 ({stageKey}) : {e.Message}");
            return WithMine(stageKey, cache?.tips ?? new List<ClearTip>());
        }
    }

    static ClearTip MaskTip(ClearTip t) => new ClearTip
    {
        uid = t.uid, nickname = MaskNow(t.nickname), text = MaskNow(t.text), updatedAt = t.updatedAt, pinned = t.pinned,
    };

    // 팁 남기기 (클리어한 스테이지만). 성공하면 true
    public static async UniTask<bool> UploadAsync(string stageKey, string rawText)
    {
        string text = MaskNow(Normalize(rawText));
        if (string.IsNullOrEmpty(text) || !CanWrite(stageKey))
            return false;
        if (UploadForTest != null)
        {
            caches.Remove(stageKey);
            var tip = new ClearTip { uid = RankingManager.UidForTest ?? "test", nickname = RankingManager.GetMyNickname(RankingManager.UidForTest), text = text };
            bool ok = UploadForTest(stageKey, tip);
            if (ok)
                myPosted[stageKey] = new ClearTip { uid = tip.uid, nickname = tip.nickname, text = text };
            return ok;
        }
        try
        {
            string uid = await FirebaseManager.Instance.firebaseAuth.EnsureSignedInAsync();
            if (string.IsNullOrEmpty(uid))
                return false;
            var dic = new Dictionary<string, object>
            {
                { NICKNAME_KEY, RankingManager.GetMyNickname(uid) },
                { TEXT_KEY, text },
                { UPDATED_AT_KEY, Firebase.Database.ServerValue.Timestamp },
            };
            await FirebaseDatabaseConfig.Ranking.RootReference.Child(TIPS_PATH).Child(stageKey).Child(uid).SetValueAsync(dic).AsUniTask();
            caches.Remove(stageKey);
            myPosted[stageKey] = new ClearTip { uid = uid, nickname = (string)dic[NICKNAME_KEY], text = text };
            Debug.Log($"[ClearTipManager] 팁 올림 ({stageKey}) {text}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[ClearTipManager] 팁 올리기 실패 ({stageKey}) : {e.Message}");
            return false;
        }
    }

    static async UniTask LoadBannedWordsAsync()
    {
        if (bannedWords != null)
            return;
        try
        {
            var snap = await FirebaseDatabaseConfig.Ranking.RootReference.Child(CONFIG_PATH).Child(BANNED_KEY).GetValueAsync().AsUniTask();
            bannedWords = ParseBannedWords(snap.Value as string);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[ClearTipManager] 금칙어 불러오기 실패 : {e.Message}");
            bannedWords = new List<string>();
        }
    }

    static long ToLong(object value) => value == null ? 0 : Convert.ToInt64(value);

    // 테스트용
    public static void ClearCacheForTest()
    {
        caches.Clear();
        bannedWords = null;
        myPosted.Clear();
    }
}

[Serializable]
public class ClearTip
{
    public string uid;
    public string nickname;
    public string text;
    public long updatedAt;
    public bool pinned;

    public string DisplayText => $"{nickname} : {text}";
}
