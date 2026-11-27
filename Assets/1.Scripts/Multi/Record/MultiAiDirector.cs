using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

// (2026-11-27) 멀티 AI 봇 = 플레이 기록 재생 (방장만 움직임, 방장이 바뀌면 새 방장이 같은 기록으로 이어서 — 봇 정보에 기록 칸 aiSlot)
// - 채우기: 빠른 매칭 방이 MultiGame.AI_FILL_WAIT_SEC 안에 안 차면 남은 자리를 봇으로 (기록 목록에서 MultiGame.PickAiRecords, 봇은 Ready)
// - 진행: 판 시작부터 지난 시간에 맞는 장면(MultiGame.FrameAt)을 AI_TICK_SEC마다 올림 → 기록이 끝난 시각이면 클리어 = 도착 / 아니면 끝남
// 화면에선 사람과 똑같이 보임 (닉네임은 랜덤, 기록 주인 정보 없음)
public class MultiAiDirector
{
    readonly Dictionary<int, PlayRecord> cache = new Dictionary<int, PlayRecord>();
    readonly HashSet<int> loading = new HashSet<int>();
    readonly System.Random rng = new System.Random();
    bool filling;
    float tickTimer;

    public bool IsFilling => filling;

    public async UniTask FillAsync(IMultiBackend backend, MultiRoomState room)
    {
        if (filling || room == null)
            return;
        int need = MultiGame.ROOM_SIZE - room.players.Count;
        if (need <= 0)
            return;
        filling = true;
        try
        {
            List<PlayRecordSummary> index = await PlayRecordStore.Instance.FetchIndexAsync();
            List<PlayRecordSummary> picks = MultiGame.PickAiRecords(index, need, rng);
            if (picks.Count == 0)
            {
                Debug.LogWarning("[MultiAI] 봇으로 쓸 플레이 기록이 없음");
                return;
            }
            for (int i = 0; i < picks.Count; i++)
            {
                string aiUid = $"{MultiGame.AI_UID_PREFIX}{room.roomId}_{i}_{rng.Next(100000)}";
                await backend.AddAiPlayerAsync(room.roomId, aiUid, MultiGame.RandomAiNickname(rng), picks[i].slot);
            }
            Debug.Log($"[MultiAI] {room.roomId}번 방 AI 봇 {picks.Count}명 추가");
        }
        finally
        {
            filling = false;
        }
    }

    // 방장이 매 프레임 부름 (AI_TICK_SEC마다 실제로 올림)
    public void Tick(IMultiBackend backend, MultiRoomState room, float deltaTime)
    {
        tickTimer += deltaTime;
        if (tickTimer < MultiGame.AI_TICK_SEC || room == null || room.fullAtMs <= 0)
            return;
        tickTimer = 0f;
        float elapsed = (backend.ServerNowMs - room.StartAtMs) / 1000f;
        if (elapsed < 0f)
            return;
        foreach (MultiPlayerState bot in room.players.Where(p => p.ai && !p.ended))
        {
            PlayRecord record = Get(bot.aiSlot);
            if (record == null)
                continue;
            if (elapsed >= record.playTime)
            {
                if (record.cleared && !bot.HasReached)
                    backend.ReportReachAsync(room.roomId, bot.uid).Forget();
                backend.ReportEndAsync(room.roomId, bot.uid).Forget();
                continue;
            }
            PlayRecord.Frame frame = MultiGame.FrameAt(record, elapsed);
            if (frame == null || (frame.wave == bot.wave && frame.kingLv == bot.kingLv && frame.minions == bot.minions))
                continue;
            backend.UpdateStatusAsync(room.roomId, new MultiStatus
            {
                wave = frame.wave, kingLv = frame.kingLv, heroKey = frame.heroKey, minions = frame.minions, board = frame.board,
            }, bot.uid).Forget();
        }
    }

    PlayRecord Get(int slot)
    {
        if (slot < 0)
            return null;
        if (cache.TryGetValue(slot, out PlayRecord r))
            return r;
        if (loading.Add(slot))
            Load(slot).Forget();
        return null;
    }

    async UniTaskVoid Load(int slot)
    {
        PlayRecord r = await PlayRecordStore.Instance.FetchRecordAsync(slot);
        loading.Remove(slot);
        if (r != null)
            cache[slot] = r;
    }
}
