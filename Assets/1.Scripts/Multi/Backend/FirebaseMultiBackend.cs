using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

// (2026-11-27) 멀티 서버 = Firebase Realtime Database 멀티 전용 인스턴스 (FirebaseDatabaseConfig.Multi, 보안 규칙 = firebase/multi.rules.json)
// 경로
//   counter = 마지막 방 번호 (트랜잭션 +1 → 방 번호 1, 2, 3 … 고유)
//   lobby/roomId = 빠른 매칭으로 지금 사람을 모으는 공개 방 (차면 다음 사람이 새 방으로 바꿈)
//   codes/{7자리 코드} = 방 번호 (방을 만든 사람이 만듦, 친구는 코드입력으로 입장)
//   rooms/{방 번호} = { createdAt, code, isPrivate(안 씀, 모든 방 같음), seats{uid:true}(자리, 트랜잭션 8명), fullAt(방장이 8명 차면 서버 시각 = 시작 기준),
//                     startAt(방장이 Ready 수마다 다시 씀), winnerUid(가장 먼저 도착, 트랜잭션),
//                     players/{uid} = { nickname, ready, joinedAt, online, wave, kingLv, heroKey, minions, board, reachedAt, ended, endedAt } }
// 방장 = online인 사람 중 가장 먼저 들어온 사람 (MultiRoomState.HostUid, 방장이 나가거나 끊기면 다음 사람) → fullAt·startAt·오래된 방 정리 (MultiMatchManager)
// 지우기: 방은 시작(fullAt, 안 찼으면 createdAt) 5시간 뒤 삭제 (방장이 될 때 CleanupOldRoomsAsync, 규칙도 5시간 지난 방만 삭제 허용). 코드도 같이 지움
// 대기 중 앱이 꺼지면 자리·유저를 비움(onDisconnect). 방이 차면 그 대신 online = false
public class FirebaseMultiBackend : IMultiBackend
{
    const string COUNTER_PATH = "counter";
    const string LOBBY_ROOM_PATH = "lobby/roomId";
    const string CODES_PATH = "codes";
    const string ROOMS_PATH = "rooms";
    const int CLEANUP_BATCH = 20;
    const int JOIN_RETRY = 4;

    string uid;
    long serverOffsetMs;
    DatabaseReference listenRef;
    EventHandler<ValueChangedEventArgs> listenHandler;
    bool offsetListening;

    static Firebase.Database.FirebaseDatabase Db => FirebaseDatabaseConfig.Multi;
    static DatabaseReference Root => Db.RootReference;
    static DatabaseReference Room(string roomId) => Root.Child(ROOMS_PATH).Child(roomId);
    DatabaseReference Me(string roomId) => Room(roomId).Child("players").Child(uid);
    static DatabaseReference Player(string roomId, string who) => Room(roomId).Child("players").Child(who);
    DatabaseReference MySeat(string roomId) => Room(roomId).Child("seats").Child(uid);

    public string MyUid => uid;
    public long ServerNowMs => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + serverOffsetMs;

    async UniTask<bool> EnsureUid()
    {
        if (!string.IsNullOrEmpty(uid))
            return true;
        if (FirebaseManager.Instance == null || FirebaseManager.Instance.firebaseAuth == null)
            return false;
        uid = await FirebaseManager.Instance.firebaseAuth.EnsureSignedInAsync();
        if (!offsetListening && !string.IsNullOrEmpty(uid))
        {
            offsetListening = true;
            Db.GetReference(".info/serverTimeOffset").ValueChanged += (s, e) =>
            {
                if (e.Snapshot?.Value != null)
                    serverOffsetMs = Convert.ToInt64(e.Snapshot.Value);
            };
        }
        return !string.IsNullOrEmpty(uid);
    }

    // ---- 방 만들기 (번호 + 코드)
    async UniTask<string> NewRoomAsync(bool isPrivate)
    {
        long index = 0;
        await Root.Child(COUNTER_PATH).RunTransaction(data =>
        {
            index = (data.Value != null ? Convert.ToInt64(data.Value) : 0) + 1;
            data.Value = index;
            return TransactionResult.Success(data);
        }).AsUniTask();
        string roomId = index.ToString();
        string code = null;
        var rng = new System.Random(Guid.NewGuid().GetHashCode());
        for (int i = 0; i < 6 && code == null; i++)
        {
            string tryCode = rng.Next(1000000, 10000000).ToString();
            bool taken = false;
            await Root.Child(CODES_PATH).Child(tryCode).RunTransaction(data =>
            {
                if (data.Value != null) { taken = true; return TransactionResult.Abort(); }
                taken = false;
                data.Value = roomId;
                return TransactionResult.Success(data);
            }).AsUniTask();
            if (!taken)
                code = tryCode;
        }
        await Room(roomId).UpdateChildrenAsync(new Dictionary<string, object>
        {
            { "createdAt", ServerValue.Timestamp },
            { "code", code ?? "" },
            { "isPrivate", isPrivate },
        }).AsUniTask();
        Debug.Log($"[Multi] {roomId}번 방 생성 (코드 {code}, {(isPrivate ? "방만들기" : "빠른 매칭")})");
        return roomId;
    }

    // 자리 잡기: 들어가면 인원 수, 꽉 찼으면 -1
    async UniTask<int> TakeSeatAsync(string roomId, string who = null)
    {
        string myUid = who ?? uid;
        int count = -1;
        await Room(roomId).Child("seats").RunTransaction(data =>
        {
            var seats = data.Value as Dictionary<string, object>;
            seats = seats != null ? new Dictionary<string, object>(seats) : new Dictionary<string, object>();
            if (!seats.ContainsKey(myUid))
            {
                if (seats.Count >= MultiGame.ROOM_SIZE)
                {
                    count = -1;
                    return TransactionResult.Abort();
                }
                seats[myUid] = true;
            }
            count = seats.Count;
            data.Value = seats;
            return TransactionResult.Success(data);
        }).AsUniTask();
        return count;
    }

    // 자리를 잡은 뒤 유저 정보 + 끊김 처리
    async UniTask EnterRoomAsync(string roomId, string nickname)
    {
        await Me(roomId).SetValueAsync(new Dictionary<string, object>
        {
            { "nickname", nickname },
            { "ready", false },
            { "online", true },
            { "joinedAt", ServerValue.Timestamp },
        }).AsUniTask();
        // 차기 전에 앱이 꺼지면 자리·유저 비움 (차면 Listen에서 online = false로 바꿈)
        await Me(roomId).OnDisconnect().RemoveValue().AsUniTask();
        await MySeat(roomId).OnDisconnect().RemoveValue().AsUniTask();
    }

    static async UniTask<bool> IsStartedAsync(string roomId)
    {
        DataSnapshot full = await Room(roomId).Child("fullAt").GetValueAsync().AsUniTask();
        return full.Exists;
    }

    public async UniTask<MultiJoinResult> JoinAsync(string nickname)
    {
        try
        {
            if (!await EnsureUid())
                return MultiJoinResult.Fail("Multi_JoinFailed");
            for (int i = 0; i < JOIN_RETRY; i++)
            {
                DataSnapshot lobby = await Root.Child(LOBBY_ROOM_PATH).GetValueAsync().AsUniTask();
                string roomId = lobby.Value != null ? Convert.ToString(lobby.Value) : null;
                if (string.IsNullOrEmpty(roomId) || await IsStartedAsync(roomId))
                    roomId = await SwapLobbyRoomAsync(roomId);
                int count = await TakeSeatAsync(roomId);
                if (count < 0)
                {
                    await SwapLobbyRoomAsync(roomId); // 꽉 참 → 다음 사람부터 새 방
                    continue;
                }
                await EnterRoomAsync(roomId, nickname);
                Debug.Log($"[Multi] {roomId}번 방 참여 ({count}/{MultiGame.ROOM_SIZE})");
                return MultiJoinResult.Success(roomId);
            }
            return MultiJoinResult.Fail("Multi_JoinFailed");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Multi] 빠른 매칭 실패 : {e.Message}");
            return MultiJoinResult.Fail("Multi_JoinFailed");
        }
    }

    // 대기열 방이 expected(없음·찬 방)면 새 공개 방으로 바꿈. 이미 다른 사람이 바꿨으면 그 방
    async UniTask<string> SwapLobbyRoomAsync(string expected)
    {
        string newRoom = await NewRoomAsync(false);
        string result = newRoom;
        await Root.Child(LOBBY_ROOM_PATH).RunTransaction(data =>
        {
            string cur = data.Value != null ? Convert.ToString(data.Value) : null;
            if (!string.IsNullOrEmpty(cur) && cur != expected)
            {
                result = cur;
                return TransactionResult.Abort();
            }
            data.Value = newRoom;
            result = newRoom;
            return TransactionResult.Success(data);
        }).AsUniTask();
        return result; // 안 쓴 새 방은 5시간 뒤 정리
    }

    public async UniTask<MultiJoinResult> CreateRoomAsync(string nickname)
    {
        try
        {
            if (!await EnsureUid())
                return MultiJoinResult.Fail("Multi_JoinFailed");
            // (2026-11-27) 방만들기 = 새 방을 만들어 빠른 매칭 대기열의 방으로 (다른 방과 똑같음, 코드로도 입장)
            string roomId = await NewRoomAsync(false);
            await Root.Child(LOBBY_ROOM_PATH).SetValueAsync(roomId).AsUniTask();
            if (await TakeSeatAsync(roomId) < 0)
                return MultiJoinResult.Fail("Multi_JoinFailed");
            await EnterRoomAsync(roomId, nickname);
            return MultiJoinResult.Success(roomId);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Multi] 방만들기 실패 : {e.Message}");
            return MultiJoinResult.Fail("Multi_JoinFailed");
        }
    }

    public async UniTask<MultiJoinResult> JoinByCodeAsync(string nickname, string code)
    {
        try
        {
            if (!MultiGame.IsValidCode(code))
                return MultiJoinResult.Fail("Multi_CodeNotFound");
            if (!await EnsureUid())
                return MultiJoinResult.Fail("Multi_JoinFailed");
            DataSnapshot snap = await Root.Child(CODES_PATH).Child(code).GetValueAsync().AsUniTask();
            string roomId = snap.Value != null ? Convert.ToString(snap.Value) : null;
            if (string.IsNullOrEmpty(roomId))
                return MultiJoinResult.Fail("Multi_CodeNotFound");
            if (await IsStartedAsync(roomId))
                return MultiJoinResult.Fail("Multi_RoomStarted");
            if (await TakeSeatAsync(roomId) < 0)
                return MultiJoinResult.Fail("Multi_RoomFull");
            await EnterRoomAsync(roomId, nickname);
            return MultiJoinResult.Success(roomId);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Multi] 코드 입장 실패 : {e.Message}");
            return MultiJoinResult.Fail("Multi_JoinFailed");
        }
    }

    // ---- 방장
    public async UniTask AddAiPlayerAsync(string roomId, string aiUid, string nickname, int recordSlot)
    {
        try
        {
            if (await TakeSeatAsync(roomId, aiUid) < 0)
                return;
            await Player(roomId, aiUid).SetValueAsync(new Dictionary<string, object>
            {
                { "nickname", nickname },
                { "ready", true },
                { "online", true },
                { "ai", true },
                { "aiSlot", recordSlot },
                { "wave", 1 },
                { "joinedAt", ServerValue.Timestamp },
            }).AsUniTask();
        }
        catch (Exception e) { Debug.LogWarning($"[Multi] AI 봇 추가 실패 : {e.Message}"); }
    }

    public async UniTask LeaveGameAsync(string roomId)
    {
        if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(roomId))
            return;
        try { await Me(roomId).Child("online").SetValueAsync(false).AsUniTask(); }
        catch (Exception e) { Debug.LogWarning($"[Multi] 나가기(online) 실패 : {e.Message}"); }
    }

    public async UniTask SetFullAtAsync(string roomId)
    {
        try { await Room(roomId).Child("fullAt").SetValueAsync(ServerValue.Timestamp).AsUniTask(); }
        catch (Exception e) { Debug.LogWarning($"[Multi] fullAt 실패 : {e.Message}"); }
    }

    public async UniTask SetStartAtAsync(string roomId, long startAtMs)
    {
        try { await Room(roomId).Child("startAt").SetValueAsync(startAtMs).AsUniTask(); }
        catch (Exception e) { Debug.LogWarning($"[Multi] startAt 실패 : {e.Message}"); }
    }

    // 시작 5시간이 지난 방 지우기 (오래된 순으로 최대 CLEANUP_BATCH개, 코드도 같이, 실패해도 무시)
    public async UniTask CleanupOldRoomsAsync()
    {
        try
        {
            long now = ServerNowMs;
            DataSnapshot snap = await Root.Child(ROOMS_PATH).OrderByChild("createdAt")
                .EndAt(now - MultiGame.ROOM_LIFETIME_MS).LimitToFirst(CLEANUP_BATCH).GetValueAsync().AsUniTask();
            foreach (DataSnapshot room in snap.Children)
            {
                if (!MultiGame.IsExpired(ToLong(room.Child("createdAt").Value), ToLong(room.Child("fullAt").Value), now))
                    continue;
                string code = room.Child("code").Value as string;
                if (!string.IsNullOrEmpty(code))
                    await Root.Child(CODES_PATH).Child(code).RemoveValueAsync().AsUniTask();
                await room.Reference.RemoveValueAsync().AsUniTask();
                Debug.Log($"[Multi] 오래된 방 {room.Key} 삭제");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Multi] 오래된 방 정리 실패 : {e.Message}");
        }
    }

    public async UniTask LeaveAsync(string roomId)
    {
        if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(roomId))
            return;
        try
        {
            if (await IsStartedAsync(roomId))
                return; // 시작한 방은 자리 유지
            await MySeat(roomId).RemoveValueAsync().AsUniTask();
            await Me(roomId).RemoveValueAsync().AsUniTask();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Multi] 나가기 실패 : {e.Message}");
        }
    }

    public void Listen(string roomId, Action<MultiRoomState> onChanged)
    {
        StopListen();
        listenRef = Root.Child(ROOMS_PATH).Child(roomId);
        bool canceledDisconnect = false;
        listenHandler = (s, e) =>
        {
            if (e.DatabaseError != null)
            {
                Debug.LogWarning($"[Multi] 방 구독 오류 : {e.DatabaseError.Message}");
                return;
            }
            MultiRoomState state = Parse(roomId, e.Snapshot);
            if (state.fullAtMs > 0 && !canceledDisconnect && !string.IsNullOrEmpty(uid))
            {
                canceledDisconnect = true; // 시작한 판은 끊겨도 자리 유지, 대신 online = false (방장이 다음 사람으로)
                Me(roomId).OnDisconnect().Cancel();
                MySeat(roomId).OnDisconnect().Cancel();
                Me(roomId).Child("online").OnDisconnect().SetValue(false);
                Me(roomId).Child("online").SetValueAsync(true);
            }
            onChanged?.Invoke(state);
        };
        listenRef.ValueChanged += listenHandler;
    }

    public void StopListen()
    {
        if (listenRef != null && listenHandler != null)
            listenRef.ValueChanged -= listenHandler;
        listenRef = null;
        listenHandler = null;
    }

    public async UniTask SetReadyAsync(string roomId)
    {
        if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(roomId))
            return;
        try { await Me(roomId).Child("ready").SetValueAsync(true).AsUniTask(); }
        catch (Exception e) { Debug.LogWarning($"[Multi] Ready 실패 : {e.Message}"); }
    }

    public async UniTask UpdateStatusAsync(string roomId, MultiStatus status, string who = null)
    {
        if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(roomId) || status == null)
            return;
        try
        {
            await Player(roomId, who ?? uid).UpdateChildrenAsync(new Dictionary<string, object>
            {
                { "wave", status.wave },
                { "kingLv", status.kingLv },
                { "heroKey", status.heroKey ?? "" },
                { "minions", status.minions ?? "" },
                { "board", status.board ?? "" },
            }).AsUniTask();
        }
        catch (Exception e) { Debug.LogWarning($"[Multi] 상황 올리기 실패 : {e.Message}"); }
    }

    public async UniTask<bool> ReportReachAsync(string roomId, string who = null)
    {
        if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(roomId))
            return false;
        string myUid = who ?? uid;
        try
        {
            await Player(roomId, myUid).Child("reachedAt").SetValueAsync(ServerValue.Timestamp).AsUniTask();
            bool won = false;
            await Root.Child(ROOMS_PATH).Child(roomId).Child("winnerUid").RunTransaction(data =>
            {
                if (data.Value != null)
                {
                    won = Convert.ToString(data.Value) == myUid;
                    return TransactionResult.Abort();
                }
                data.Value = myUid;
                won = true;
                return TransactionResult.Success(data);
            }).AsUniTask();
            return won;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Multi] 도착 기록 실패 : {e.Message}");
            return false;
        }
    }

    public async UniTask ReportEndAsync(string roomId, string who = null)
    {
        if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(roomId))
            return;
        try
        {
            await Player(roomId, who ?? uid).UpdateChildrenAsync(new Dictionary<string, object>
            {
                { "ended", true },
                { "endedAt", ServerValue.Timestamp },
            }).AsUniTask();
        }
        catch (Exception e) { Debug.LogWarning($"[Multi] 종료 기록 실패 : {e.Message}"); }
    }

    static MultiRoomState Parse(string roomId, DataSnapshot snap)
    {
        var state = new MultiRoomState { roomId = roomId };
        if (snap == null || !snap.Exists)
            return state;
        state.createdAt = ToLong(snap.Child("createdAt").Value);
        state.fullAtMs = ToLong(snap.Child("fullAt").Value);
        state.winnerUid = snap.Child("winnerUid").Value as string;
        state.code = snap.Child("code").Value as string;
        state.isPrivate = snap.Child("isPrivate").Value is bool pv && pv;
        state.startAtMs = ToLong(snap.Child("startAt").Value);
        foreach (DataSnapshot p in snap.Child("players").Children)
            state.players.Add(new MultiPlayerState
            {
                uid = p.Key,
                nickname = p.Child("nickname").Value as string ?? "",
                ready = p.Child("ready").Value is bool b && b,
                online = !(p.Child("online").Value is bool on) || on,
                ai = p.Child("ai").Value is bool isAi && isAi,
                aiSlot = p.Child("aiSlot").Value != null ? (int)ToLong(p.Child("aiSlot").Value) : -1,
                joinedAt = ToLong(p.Child("joinedAt").Value),
                wave = (int)ToLong(p.Child("wave").Value),
                kingLv = (int)ToLong(p.Child("kingLv").Value),
                heroKey = p.Child("heroKey").Value as string ?? "",
                minions = p.Child("minions").Value as string ?? "",
                board = p.Child("board").Value as string ?? "",
                reachedAt = ToLong(p.Child("reachedAt").Value),
                ended = p.Child("ended").Value is bool en && en,
                endedAt = ToLong(p.Child("endedAt").Value),
            });
        state.players = state.players.OrderBy(p => p.joinedAt).ToList();
        return state;
    }

    static long ToLong(object v) => v == null ? 0 : Convert.ToInt64(v);
}
