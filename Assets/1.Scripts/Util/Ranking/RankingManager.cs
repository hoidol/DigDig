using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

// 난이도별 wave 랭킹 (Realtime Database, 랭킹 전용 인스턴스 FirebaseDatabaseConfig.RANKING_URL).
// 경로 : rankings/{난이도}/{uid} = { nickname, wave, updatedAt }
//   wave      : 표시용 최고 wave (UserStage.maxWave + 1)
//   updatedAt : 그 wave를 처음 올린 서버 시간. 같은 wave면 먼저 달성한 사람이 위
//
// 갱신 정책
// - 내 기록 업로드 : 랭킹을 요청할 때마다 확인하되, 서버보다 기록이 높거나 닉네임이 바뀐 경우에만 쓴다.
// - 목록 불러오기 : 난이도별로 10분(REFRESH_INTERVAL_SEC)에 한 번. 캔버스를 껐다 켜도 캐시를 쓴다.
//   단, 내 최고 wave가 올라가 업로드했으면 목록에 바로 보이도록 캐시를 버리고 다시 불러온다.
// - 로그인 계정(uid)이 바뀌면 캐시 전체 초기화.
public static class RankingManager
{
    public const int TOP_COUNT = 100;
    public const float REFRESH_INTERVAL_SEC = 600f;

    const string RANKINGS_PATH = "rankings";
    const string NICKNAME_KEY = "nickname";
    const string WAVE_KEY = "wave";
    const string UPDATED_AT_KEY = "updatedAt";

    class UploadedInfo
    {
        public int wave;
        public string nickname;
    }

    static readonly Dictionary<DifficultyType, RankingResult> caches = new Dictionary<DifficultyType, RankingResult>();
    static readonly Dictionary<DifficultyType, UploadedInfo> uploaded = new Dictionary<DifficultyType, UploadedInfo>();
    static readonly HashSet<DifficultyType> loading = new HashSet<DifficultyType>();
    static string cachedUid;

    // 내 기록 업로드 → (10분 지났으면) 랭킹 목록 불러오기. 실패 시 null (이전 캐시가 있으면 그걸 반환)
    public static async UniTask<RankingResult> GetRankingAsync(DifficultyType difficulty)
    {
        await UniTask.WaitWhile(() => loading.Contains(difficulty)); // 같은 난이도 중복 요청 방지
        loading.Add(difficulty);
        try
        {
            string uid = await FirebaseManager.Instance.firebaseAuth.EnsureSignedInAsync();
            if (string.IsNullOrEmpty(uid))
                return null;

            if (uid != cachedUid)
            {
                caches.Clear();
                uploaded.Clear();
                cachedUid = uid;
            }

            bool waveUpdated = await UploadMyRankAsync(difficulty, uid);

            caches.TryGetValue(difficulty, out RankingResult cache);
            bool expired = cache == null || Time.realtimeSinceStartup - cache.fetchedAt >= REFRESH_INTERVAL_SEC;
            if (!expired && !waveUpdated)
                return cache;

            RankingResult result = await FetchRankingAsync(difficulty, uid);
            if (result == null)
                return cache;

            caches[difficulty] = result;
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"[RankingManager] 랭킹 처리 실패 ({difficulty}) : {e}");
            caches.TryGetValue(difficulty, out RankingResult cache);
            return cache;
        }
        finally
        {
            loading.Remove(difficulty);
        }
    }

    // 다음 갱신까지 남은 시간(초). 캐시 없으면 0
    public static float GetRemainRefreshSec(DifficultyType difficulty)
    {
        if (!caches.TryGetValue(difficulty, out RankingResult cache))
            return 0;
        return Mathf.Max(0, REFRESH_INTERVAL_SEC - (Time.realtimeSinceStartup - cache.fetchedAt));
    }

    // 서버보다 wave가 높으면 wave/updatedAt까지, 닉네임만 다르면 닉네임만 쓴다. wave를 올렸으면 true
    static async UniTask<bool> UploadMyRankAsync(DifficultyType difficulty, string uid)
    {
        int wave = GetMyWave(difficulty);
        if (wave <= 0)
            return false; // 플레이한 적 없는 난이도는 올리지 않음

        string nickname = GetMyNickname(uid);
        if (uploaded.TryGetValue(difficulty, out UploadedInfo info) && info.wave >= wave && info.nickname == nickname)
            return false;

        var myRef = GetDifficultyRef(difficulty).Child(uid);
        var snapshot = await myRef.GetValueAsync().AsUniTask();
        int serverWave = snapshot.Exists ? ToInt(snapshot.Child(WAVE_KEY).Value) : 0;
        string serverNickname = snapshot.Exists ? snapshot.Child(NICKNAME_KEY).Value as string : null;

        var dic = new Dictionary<string, object>();
        if (serverNickname != nickname)
            dic[NICKNAME_KEY] = nickname;
        bool waveUpdated = wave > serverWave; // 다른 기기에서 더 높게 올린 기록은 유지
        if (waveUpdated)
        {
            dic[WAVE_KEY] = wave;
            dic[UPDATED_AT_KEY] = Firebase.Database.ServerValue.Timestamp;
        }

        if (dic.Count > 0)
        {
            if (!dic.ContainsKey(NICKNAME_KEY))
                dic[NICKNAME_KEY] = nickname; // 보안 규칙 검증(필수 필드)용으로 함께 보냄
            await myRef.UpdateChildrenAsync(dic).AsUniTask();
            Debug.Log($"[RankingManager] 내 기록 업로드 ({difficulty}) wave {Mathf.Max(wave, serverWave)} / {nickname}");
        }

        uploaded[difficulty] = new UploadedInfo { wave = Mathf.Max(wave, serverWave), nickname = nickname };
        return waveUpdated;
    }

    static async UniTask<RankingResult> FetchRankingAsync(DifficultyType difficulty, string uid)
    {
        var snapshot = await GetDifficultyRef(difficulty)
            .OrderByChild(WAVE_KEY)
            .LimitToLast(TOP_COUNT)
            .GetValueAsync().AsUniTask();

        var ranks = new List<RankData>();
        foreach (var child in snapshot.Children)
        {
            ranks.Add(new RankData
            {
                uid = child.Key,
                nickname = child.Child(NICKNAME_KEY).Value as string ?? "",
                wave = ToInt(child.Child(WAVE_KEY).Value),
                updatedAt = ToLong(child.Child(UPDATED_AT_KEY).Value),
            });
        }

        // RTDB는 오름차순 → wave 내림차순, 같은 wave면 먼저 달성한 순
        ranks = ranks.OrderByDescending(e => e.wave).ThenBy(e => e.updatedAt).ToList();
        for (int i = 0; i < ranks.Count; i++)
            ranks[i].num = i + 1;

        Debug.Log($"[RankingManager] 랭킹 불러오기 완료 ({difficulty}) {ranks.Count}명");
        return new RankingResult
        {
            difficulty = difficulty,
            ranks = ranks,
            my = ranks.FirstOrDefault(e => e.uid == uid),
            fetchedAt = Time.realtimeSinceStartup,
        };
    }

    // 내 최고 wave (표시용, 1부터). 플레이 기록 없으면 0
    public static int GetMyWave(DifficultyType difficulty)
    {
        string key = difficulty.ToString();
        UserStage userStage = UserDataManager.Instance.userStageManager.userStageData.userStages
            .FirstOrDefault(e => e.key == key); // GetUserStage는 없으면 생성하므로 직접 찾음
        if (userStage == null || userStage.tryCount <= 0)
            return 0;
        return userStage.maxWave + 1;
    }

    // 닉네임이 기본값(GUID)이면 Guest#uid앞4자리
    public static string GetMyNickname(string uid)
    {
        string nickname = UserDataManager.Instance.userMyInfoManager.userMyInfoData.nickname;
        if (string.IsNullOrWhiteSpace(nickname) || Guid.TryParse(nickname, out _))
            return $"Guest#{(uid ?? "????").Substring(0, Mathf.Min(4, (uid ?? "????").Length))}";
        return nickname;
    }

    static Firebase.Database.DatabaseReference GetDifficultyRef(DifficultyType difficulty)
    {
        return FirebaseDatabaseConfig.Ranking.RootReference
            .Child(RANKINGS_PATH).Child(difficulty.ToString());
    }

    static int ToInt(object value) => value == null ? 0 : Convert.ToInt32(value);
    static long ToLong(object value) => value == null ? 0 : Convert.ToInt64(value);
}

public class RankingResult
{
    public DifficultyType difficulty;
    public List<RankData> ranks;
    public RankData my; // 상위 TOP_COUNT 안에 없으면 null
    public float fetchedAt; // Time.realtimeSinceStartup
}

public class RankData
{
    public int num;
    public string uid;
    public string nickname;
    public int wave;
    public long updatedAt;
}
