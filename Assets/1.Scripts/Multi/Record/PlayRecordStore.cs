using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

// (2026-11-27) 플레이 기록 서버 (공용 DB FirebaseDatabaseConfig.Common, 보안 규칙 firebase/common.rules.json)
// 경로: meta/nextIndex = 지금까지 올린 판 수 (트랜잭션 +1) · index/{칸} = 요약(PlayRecordSummary) · records/{칸} = 기록 JSON 문자열
// 칸 = (번호 - 1) % MAX_RECORDS → 100판까지 쌓이고, 넘으면 가장 오래된 판 자리를 덮어씀 (AI 봇은 이 중 랜덤으로 최대 7판씩 받음)
// (2026-11-27) 100칸이 다 찬 뒤부터 유저당 최대 MAX_PER_USER칸: 이미 그만큼 있으면 새 칸 대신 그 유저의 가장 오래된 칸을 덮어씀 (owner = uid 해시). 다 차기 전엔 제한 없음
// 장면이 MIN_FRAMES보다 적은 판(너무 짧은 판)은 안 올림
// 에디터·테스트 = FakePlayRecordStore (메모리 + 기록이 없으면 만든 예시 기록)
public interface IPlayRecordStore
{
    UniTask<bool> UploadAsync(PlayRecord record);
    UniTask<List<PlayRecordSummary>> FetchIndexAsync();
    UniTask<PlayRecord> FetchRecordAsync(int slot);
}

public static class PlayRecordStore
{
    public const int MAX_RECORDS = 100;
    public const int MAX_PER_USER = 5;
    public const int MIN_FRAMES = 3;

    // uid → 해시 (앞 16자리, 되돌릴 수 없음)
    public static string OwnerHash(string uid)
    {
        if (string.IsNullOrEmpty(uid))
            return "";
        using var sha = System.Security.Cryptography.SHA256.Create();
        byte[] h = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(uid));
        return BitConverter.ToString(h, 0, 8).Replace("-", "").ToLowerInvariant();
    }

    // 올릴 칸: 100칸이 다 찼고 그 유저 칸이 MAX_PER_USER개 이상이면 그중 가장 오래된 칸(번호 안 씀), 아니면 -1 = 다음 번호의 칸 (씬 없이 계산)
    public static int PickOwnSlot(IEnumerable<PlayRecordSummary> index, string owner)
    {
        if (string.IsNullOrEmpty(owner))
            return -1;
        var all = index.Where(s => s != null).ToList();
        if (all.Count < MAX_RECORDS)
            return -1; // 다 차기 전엔 제한 없음
        var mine = all.Where(s => s.owner == owner).OrderBy(s => s.uploadedAt).ToList();
        return mine.Count >= MAX_PER_USER ? mine[0].slot : -1;
    }

    // 테스트·에디터: true면 가짜 (기본 = 에디터이거나 Firebase가 없으면 가짜)
    public static bool? ForceFake;
    static IPlayRecordStore instance;
    public static IPlayRecordStore Instance
    {
        get
        {
            bool fake = ForceFake ?? (Application.isEditor || FirebaseManager.Instance == null);
            if (instance == null || (fake != (instance is FakePlayRecordStore)))
                instance = fake ? new FakePlayRecordStore() : new FirebasePlayRecordStore();
            return instance;
        }
    }

    public static int SlotOf(long number) => (int)((number - 1) % MAX_RECORDS);

    public static PlayRecordSummary Summarize(PlayRecord r, int slot, string owner = null, long uploadedAt = 0) => new PlayRecordSummary
    {
        owner = owner, uploadedAt = uploadedAt,
        slot = slot, id = r.id, mode = r.mode, stageOrder = r.stageOrder, difficulty = r.difficulty,
        finalWave = r.finalWave, cleared = r.cleared, playTime = r.playTime, startedAt = r.startedAt, frameCount = r.frames.Count,
    };
}

public class FirebasePlayRecordStore : IPlayRecordStore
{
    static Firebase.Database.DatabaseReference Root => FirebaseDatabaseConfig.Common.RootReference;

    public async UniTask<bool> UploadAsync(PlayRecord record)
    {
        try
        {
            if (record == null || record.frames.Count < PlayRecordStore.MIN_FRAMES)
                return true; // 너무 짧은 판은 안 올림 (올린 것으로 침)
            string uid = FirebaseManager.Instance != null ? await FirebaseManager.Instance.firebaseAuth.EnsureSignedInAsync() : null;
            if (string.IsNullOrEmpty(uid))
                return false;
            string owner = PlayRecordStore.OwnerHash(uid);
            int slot = PlayRecordStore.PickOwnSlot(await FetchIndexAsync(), owner);
            if (slot < 0)
            {
                long number = 0;
                await Root.Child("meta/nextIndex").RunTransaction(data =>
                {
                    number = (data.Value != null ? Convert.ToInt64(data.Value) : 0) + 1;
                    data.Value = number;
                    return Firebase.Database.TransactionResult.Success(data);
                }).AsUniTask();
                slot = PlayRecordStore.SlotOf(number);
            }
            await Root.Child("records").Child(slot.ToString()).SetValueAsync(JsonUtility.ToJson(record)).AsUniTask();
            await Root.Child("index").Child(slot.ToString()).SetRawJsonValueAsync(JsonUtility.ToJson(PlayRecordStore.Summarize(record, slot, owner, ServerClock.NowMs))).AsUniTask();
            Debug.Log($"[PlayRecord] 올림 {record.id} → {slot}칸");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PlayRecord] 올리기 실패 : {e.Message}");
            return false;
        }
    }

    public async UniTask<List<PlayRecordSummary>> FetchIndexAsync()
    {
        var list = new List<PlayRecordSummary>();
        try
        {
            var snap = await Root.Child("index").GetValueAsync().AsUniTask();
            foreach (var child in snap.Children)
            {
                PlayRecordSummary s = JsonUtility.FromJson<PlayRecordSummary>(child.GetRawJsonValue());
                if (s != null)
                    list.Add(s);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PlayRecord] 목록 받기 실패 : {e.Message}");
        }
        return list;
    }

    public async UniTask<PlayRecord> FetchRecordAsync(int slot)
    {
        try
        {
            var snap = await Root.Child("records").Child(slot.ToString()).GetValueAsync().AsUniTask();
            return snap.Value is string json ? JsonUtility.FromJson<PlayRecord>(json) : null;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PlayRecord] 기록 받기 실패 ({slot}) : {e.Message}");
            return null;
        }
    }
}

// 에디터·테스트용: 메모리에 300칸 돌려쓰기. 비어 있으면 예시 기록(만든 값) 몇 판
public class FakePlayRecordStore : IPlayRecordStore
{
    public static readonly Dictionary<int, PlayRecord> records = new Dictionary<int, PlayRecord>();
    public static readonly Dictionary<int, PlayRecordSummary> index = new Dictionary<int, PlayRecordSummary>();
    public static long count;
    public static string owner = "me"; // 테스트에서 바꿔 여러 유저 흉내
    static long fakeClock;

    public static void Clear()
    {
        records.Clear();
        index.Clear();
        count = 0;
    }

    public UniTask<bool> UploadAsync(PlayRecord record)
    {
        if (record == null || record.frames.Count < PlayRecordStore.MIN_FRAMES)
            return UniTask.FromResult(true);
        int slot = PlayRecordStore.PickOwnSlot(index.Values, owner);
        if (slot < 0)
            slot = PlayRecordStore.SlotOf(++count);
        records[slot] = JsonUtility.FromJson<PlayRecord>(JsonUtility.ToJson(record)); // 복사
        index[slot] = PlayRecordStore.Summarize(record, slot, owner, ++fakeClock);
        return UniTask.FromResult(true);
    }

    public UniTask<List<PlayRecordSummary>> FetchIndexAsync()
    {
        if (records.Count == 0)
        {
            string prev = owner;
            for (int i = 0; i < 8; i++)
            {
                owner = $"sample{i}";
                UploadAsync(MakeSample(i));
            }
            owner = prev;
        }
        return UniTask.FromResult(index.Values.ToList());
    }

    public UniTask<PlayRecord> FetchRecordAsync(int slot) =>
        UniTask.FromResult(records.TryGetValue(slot, out PlayRecord r) ? r : null);

    // 예시 기록: 6~12초마다 wave +1, 25~50 wave까지 (짝수 번호는 클리어)
    public static PlayRecord MakeSample(int seed, float secondsPerWave = 8f)
    {
        var rng = new System.Random(seed + 1);
        var r = new PlayRecord { id = $"sample{seed}", mode = PlayRecord.MODE_SINGLE, stageOrder = MultiGame.MULTI_STAGE_ORDER, difficulty = MultiGame.MULTI_DIFFICULTY };
        int last = seed % 2 == 0 ? MultiGame.GOAL_WAVE : 20 + rng.Next(25);
        float t = 0;
        var minions = new List<(string, int)>();
        string[] keys = { "Random", "Flame", "Ice", "Lightning", "Orbit", "Bounce" };
        for (int w = 1; w <= last; w++)
        {
            if (minions.Count < 8) minions.Add((keys[rng.Next(keys.Length)], 1));
            else { int i = rng.Next(minions.Count); minions[i] = (minions[i].Item1, Math.Min(5, minions[i].Item2 + 1)); }
            var board = new MultiBoard();
            for (int i = 0; i < minions.Count; i++)
                board.cells.Add(new MultiBoard.Cell { x = i % 6, y = i / 6, minion = minions[i].Item1, lv = minions[i].Item2 });
            r.frames.Add(new PlayRecord.Frame
            {
                t = t, wave = w, kingLv = Math.Min(12, w / 4), heroKey = w >= 4 ? "Knight" : "", hp = 1f, diceCoin = 5,
                minions = MultiGame.FormatMinions(minions), board = JsonUtility.ToJson(board),
            });
            t += secondsPerWave * (0.75f + (float)rng.NextDouble() * 0.5f);
        }
        r.playTime = t;
        r.finalWave = last;
        r.cleared = last >= MultiGame.GOAL_WAVE;
        return r;
    }
}
