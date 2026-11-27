using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

// (2026-11-27) 에디터·테스트용 가짜 멀티 서버 (에디터엔 Firebase 네이티브가 없음)
// 들어가면 봇이 botJoinInterval초마다 한 명씩 들어와 8명을 채우고, 카운트다운 중 랜덤하게 Ready
// 시작 뒤 봇은 botWaveInterval초마다 wave +1 (미니언·슬라임 레벨도 조금씩), 낮은 확률로 탈락, wave 50이면 도착
// 시간은 실제 시간(unscaled). 방 번호는 1부터, 코드는 7자리
// 방장: 내가 먼저 들어간 방(빠른 매칭·방만들기)은 내가 방장 → fullAt·startAt은 MultiMatchManager가 씀
//   코드입력 = 봇들이 먼저 있는 남의 방(방장 = 봇) → 가짜 서버가 방장 일을 대신 함. SimulateLeave로 방장이 나가는 상황
public class FakeMultiBackend : IMultiBackend
{
    public static float botJoinInterval = 1.5f;   // 테스트에서 줄일 수 있음
    public static Vector2 botReadyDelay = new Vector2(1f, 5f);
    public static float botWaveInterval = 6f;
    public static float botOutChancePerWave = 0.02f;

    static int nextIndex;
    static readonly string[] BOT_MINIONS = { "Random", "Flame", "Ice", "Lightning", "Orbit", "Bounce", "Iron", "Pierce" };
    static readonly string[] BOT_HEROES = { "Knight", "Ranger", "Necromancer" };

    readonly string uid = "me";
    bool botHost; // 방장이 봇 (코드입력으로 들어간 방)
    MultiRoomState room;
    Action<MultiRoomState> listener;
    int version;

    public string MyUid => uid;
    public long ServerNowMs => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    MultiRoomState NewRoom(bool isPrivate) =>
        new MultiRoomState { roomId = (++nextIndex).ToString(), createdAt = ServerNowMs, isPrivate = isPrivate, code = UnityEngine.Random.Range(1000000, 10000000).ToString() };

    public UniTask<MultiJoinResult> JoinAsync(string nickname) => UniTask.FromResult(Start(nickname, NewRoom(false), 0));

    public UniTask<MultiJoinResult> CreateRoomAsync(string nickname) => UniTask.FromResult(Start(nickname, NewRoom(false), 0));

    // 코드입력: 봇 3명이 먼저 있는 방 (방장 = 봇1)
    public UniTask<MultiJoinResult> JoinByCodeAsync(string nickname, string code)
    {
        if (!MultiGame.IsValidCode(code))
            return UniTask.FromResult(MultiJoinResult.Fail("Multi_CodeNotFound"));
        MultiRoomState r = NewRoom(false);
        r.code = code;
        return UniTask.FromResult(Start(nickname, r, 3));
    }

    MultiJoinResult Start(string nickname, MultiRoomState r, int botsFirst)
    {
        version++;
        room = r;
        botHost = botsFirst > 0;
        long t = ServerNowMs;
        for (int i = 1; i <= botsFirst; i++)
            room.players.Add(new MultiPlayerState { uid = $"bot{i}", nickname = $"Bot{i}", joinedAt = t - 1000 + i, wave = 1 });
        room.players.Add(new MultiPlayerState { uid = uid, nickname = nickname, joinedAt = t, wave = 1 });
        FillBots(version, botsFirst + 1).Forget();
        return MultiJoinResult.Success(room.roomId);
    }

    // 테스트: 그 유저가 나감 (대기 중 = 자리 비움, 시작 뒤 = online false)
    public void SimulateLeave(string who)
    {
        MultiPlayerState p = room?.players.Find(x => x.uid == who);
        if (p == null)
            return;
        if (room.fullAtMs > 0)
            p.online = false;
        else
            room.players.Remove(p);
        botHost = room.HostUid != uid;
        Notify();
    }

    async UniTaskVoid FillBots(int v, int n)
    {
        while (v == version && room != null && room.players.Count < MultiGame.ROOM_SIZE)
        {
            // 간격을 기다리는 중에 botJoinInterval이 바뀌어도 바로 따름 (테스트)
            float waited = 0f;
            while (waited < botJoinInterval)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.05f), ignoreTimeScale: true);
                waited += 0.05f;
                if (v != version || room == null)
                    return;
            }
            room.players.Add(new MultiPlayerState { uid = $"bot{n}", nickname = $"Bot{n}", joinedAt = ServerNowMs + n, wave = 1 });
            n++;
            if (room.players.Count >= MultiGame.ROOM_SIZE)
            {
                if (botHost)
                    room.fullAtMs = ServerNowMs; // 남의 방: 방장(봇)이 씀. 내 방이면 MultiMatchManager(방장 = 나)가 SetFullAtAsync
                foreach (MultiPlayerState p in room.players)
                    if (p.uid != uid)
                    {
                        BotReady(v, p).Forget();
                        BotPlay(v, p).Forget();
                    }
            }
            Notify();
        }
    }

    async UniTaskVoid BotReady(int v, MultiPlayerState bot)
    {
        float delay = (float)(MultiGame.LOAD_BUFFER_MS / 1000.0) + UnityEngine.Random.Range(botReadyDelay.x, botReadyDelay.y);
        await UniTask.Delay(TimeSpan.FromSeconds(delay), ignoreTimeScale: true);
        if (v != version || room == null || UnityEngine.Random.value < 0.3f) // 30%는 Ready 안 누름
            return;
        bot.ready = true;
        if (botHost && room.fullAtMs > 0)
            room.startAtMs = MultiGame.GetStartAtMs(room.fullAtMs, room.ReadyCount); // 방장(봇)이 다시 씀
        Notify();
    }

    async UniTaskVoid BotPlay(int v, MultiPlayerState bot)
    {
        var minions = new List<(string, int)>();
        while (v == version && room != null && !bot.ended)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(botWaveInterval * UnityEngine.Random.Range(0.6f, 1.4f)), ignoreTimeScale: true);
            if (v != version || room == null)
                return;
            if (room.fullAtMs <= 0 || ServerNowMs < room.StartAtMs)
                continue;
            bot.wave++;
            if (bot.wave % 3 == 0)
                bot.slimeLv = Mathf.Min(12, bot.slimeLv + 1);
            if (bot.slimeLv >= 1 && string.IsNullOrEmpty(bot.heroKey))
                bot.heroKey = BOT_HEROES[UnityEngine.Random.Range(0, BOT_HEROES.Length)];
            if (minions.Count < 8)
                minions.Add((BOT_MINIONS[UnityEngine.Random.Range(0, BOT_MINIONS.Length)], 1));
            else
            {
                int i = UnityEngine.Random.Range(0, minions.Count);
                minions[i] = (minions[i].Item1, Mathf.Min(5, minions[i].Item2 + 1));
            }
            bot.minions = MultiGame.FormatMinions(minions);
            bot.board = MakeBoard(minions);
            if (bot.wave > MultiGame.GOAL_WAVE)
            {
                bot.reachedAt = ServerNowMs;
                bot.ended = true;
                bot.endedAt = ServerNowMs;
                room.winnerUid ??= bot.uid;
            }
            else if (UnityEngine.Random.value < botOutChancePerWave)
            {
                bot.ended = true;
                bot.endedAt = ServerNowMs;
            }
            Notify();
        }
    }

    static string MakeBoard(List<(string key, int lv)> minions)
    {
        var board = new MultiBoard();
        int i = 0;
        foreach (var m in minions)
        {
            board.cells.Add(new MultiBoard.Cell { x = i % 6, y = i / 6, minion = m.key, lv = m.lv });
            i++;
        }
        board.cells.Add(new MultiBoard.Cell { x = 3, y = 4, slime = true });
        return JsonUtility.ToJson(board);
    }

    public UniTask SetFullAtAsync(string roomId)
    {
        if (room != null && room.fullAtMs <= 0)
        {
            room.fullAtMs = ServerNowMs;
            Notify();
        }
        return UniTask.CompletedTask;
    }

    public UniTask SetStartAtAsync(string roomId, long startAtMs)
    {
        if (room != null)
        {
            room.startAtMs = startAtMs;
            Notify();
        }
        return UniTask.CompletedTask;
    }

    public UniTask CleanupOldRoomsAsync() => UniTask.CompletedTask;

    public UniTask LeaveAsync(string roomId)
    {
        version++;
        room = null;
        return UniTask.CompletedTask;
    }

    public void Listen(string roomId, Action<MultiRoomState> onChanged)
    {
        listener = onChanged;
        Notify();
    }

    public void StopListen() => listener = null;

    MultiPlayerState Me => room?.players.Find(p => p.uid == uid);

    public UniTask SetReadyAsync(string roomId)
    {
        if (Me != null)
        {
            Me.ready = true;
            if (botHost && room.fullAtMs > 0)
                room.startAtMs = MultiGame.GetStartAtMs(room.fullAtMs, room.ReadyCount);
            Notify();
        }
        return UniTask.CompletedTask;
    }

    MultiPlayerState Who(string who) => who == null ? Me : room?.players.Find(p => p.uid == who);

    public UniTask AddAiPlayerAsync(string roomId, string aiUid, string nickname, int recordSlot)
    {
        if (room != null && room.players.Count < MultiGame.ROOM_SIZE && Who(aiUid) == null)
        {
            room.players.Add(new MultiPlayerState { uid = aiUid, nickname = nickname, ready = true, ai = true, aiSlot = recordSlot, wave = 1, joinedAt = ServerNowMs });
            Notify();
        }
        return UniTask.CompletedTask;
    }

    public UniTask LeaveGameAsync(string roomId)
    {
        if (Me != null)
        {
            Me.online = false;
            Notify();
        }
        return UniTask.CompletedTask;
    }

    public UniTask UpdateStatusAsync(string roomId, MultiStatus status, string who = null)
    {
        MultiPlayerState me = Who(who);
        if (me != null && status != null)
        {
            me.wave = status.wave;
            me.slimeLv = status.slimeLv;
            me.heroKey = status.heroKey;
            me.minions = status.minions;
            me.board = status.board;
            Notify();
        }
        return UniTask.CompletedTask;
    }

    public UniTask<bool> ReportReachAsync(string roomId, string who = null)
    {
        MultiPlayerState me = Who(who);
        if (me == null)
            return UniTask.FromResult(false);
        me.reachedAt = ServerNowMs;
        room.winnerUid ??= me.uid;
        Notify();
        return UniTask.FromResult(room.winnerUid == me.uid);
    }

    public UniTask ReportEndAsync(string roomId, string who = null)
    {
        MultiPlayerState me = Who(who);
        if (me != null)
        {
            me.ended = true;
            me.endedAt = ServerNowMs;
            Notify();
        }
        return UniTask.CompletedTask;
    }

    void Notify()
    {
        if (room == null || listener == null)
            return;
        // 받는 쪽이 목록을 들고 있어도 다음 변경과 섞이지 않게 복사
        var copy = new MultiRoomState { roomId = room.roomId, createdAt = room.createdAt, fullAtMs = room.fullAtMs, winnerUid = room.winnerUid,
            code = room.code, isPrivate = room.isPrivate, startAtMs = room.startAtMs };
        foreach (MultiPlayerState p in room.players)
            copy.players.Add(new MultiPlayerState
            {
                uid = p.uid, nickname = p.nickname, ready = p.ready, joinedAt = p.joinedAt, online = p.online, ai = p.ai, aiSlot = p.aiSlot, wave = p.wave, slimeLv = p.slimeLv,
                heroKey = p.heroKey, minions = p.minions, board = p.board, reachedAt = p.reachedAt, ended = p.ended, endedAt = p.endedAt,
            });
        listener(copy);
    }
}
