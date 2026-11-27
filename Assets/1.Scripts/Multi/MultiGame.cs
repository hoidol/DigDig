using System.Collections.Generic;
using System.Linq;

// (2026-11-27) 멀티 게임 (비동기 대전, 최대 8명, Firebase Realtime Database. 실시간 동기화·Photon 없음)
// 흐름: 로비 [멀티 게임] → 안내(배속·광고 제거 상품 사용 불가) → 대기열 참여(MultiMatchManager) → 기다리는 동안 로컬 게임 가능(오른쪽 위 N/8)
//   → 8명이 차면 InGame(멀티 모드) + MultiGameUI 씬 → 8명 닉네임 + 10초 카운트다운 (Ready 1명당 1초 감소) → 모두 같은 시각에 시작
//   → (다음 단계) 다른 유저 상황·보기·순위(웨이브 50 보스를 가장 빨리 잡은 사람이 우승)·전적
// 멀티 모드 규칙: 배속 상품·광고 제거 상품 효과 없음(UserDataManager.IsGameSpeedUp·IsAdRemovedEffective), 용사 패스는 적용, 시드는 각자 랜덤, 부활 있음
// 스테이지 = 스테이지 1 보통 (MULTI_STAGE_ORDER·MULTI_DIFFICULTY)
public static class MultiGame
{
    public const int ROOM_SIZE = 8;
    public const int READY_SECONDS = 10;      // 카운트다운 시작 값
    public const int READY_REDUCE_SECONDS = 1; // Ready 1명당 줄어드는 초
    public const long PRE_START_MS = 5000;     // (2026-11-27) 방이 차면 대기 표시에 "5초 후 시작합니다" → 그다음 멀티 판으로
    public const long LOAD_BUFFER_MS = 3000;   // 씬을 불러오는 시간 (Ready 카운트다운은 그 뒤부터)
    public const int GOAL_WAVE = 50;           // 이 wave 보스를 잡으면 도착
    public const int MULTI_STAGE_ORDER = StageData.FIRST_STAGE_ORDER;
    public const DifficultyType MULTI_DIFFICULTY = DifficultyType.Normal;

    // 지금 InGame이 멀티 판인지 (MultiMatchManager가 멀티 InGame을 불러올 때 켜고, 멀티가 끝나면 끔)
    public static bool IsActive { get; set; }
    public static string RoomId { get; set; }
    public static MultiRoomState Room { get; set; }
    // 카운트다운이 끝나 게임을 시작해도 되는지 (GameManager가 기다림)
    public static bool CanStart { get; set; }

    // 시작 시각(서버 ms) = 방이 찬 시각 + "5초 후 시작" + 씬 로딩 + 10초 - Ready 수 × 1초 (씬 없이 계산)
    public static long GetStartAtMs(long fullAtMs, int readyCount)
    {
        int ready = System.Math.Max(0, System.Math.Min(readyCount, ROOM_SIZE));
        return fullAtMs + PRE_START_MS + LOAD_BUFFER_MS + (READY_SECONDS - READY_REDUCE_SECONDS * ready) * 1000L;
    }

    // 방이 찬 뒤 멀티 판으로 넘어가는 시각
    public static long GetEnterAtMs(long fullAtMs) => fullAtMs + PRE_START_MS;

    // AI로 채우기까지 남은 초 (방 생성부터 AI_FILL_WAIT_SEC, 0 이상) (씬 없이 계산)
    public static float GetAiFillRemain(long createdAt, long nowMs, float waitSec) =>
        createdAt <= 0 ? waitSec : System.Math.Max(0f, waitSec - (nowMs - createdAt) / 1000f);

    // 화면 이름: AI 봇이면 뒤에 [AI]
    public static string DisplayName(MultiPlayerState p) =>
        p == null ? "" : p.ai ? $"{p.nickname} <color=#8FD3FF>[AI]</color>" : p.nickname;

    // 남은 초 (올림, 0 이상, 최대 READY_SECONDS)
    public static int GetRemainSeconds(long startAtMs, long nowMs)
    {
        long remain = startAtMs - nowMs;
        if (remain <= 0)
            return 0;
        return (int)System.Math.Min(READY_SECONDS, (remain + 999) / 1000);
    }

    public const long ROOM_LIFETIME_MS = 5L * 60 * 60 * 1000; // 방은 시작(차기 전이면 생성) 5시간 뒤 지움

    // 지울 때가 됐는지 (씬 없이 계산)
    public static bool IsExpired(long createdAt, long fullAt, long nowMs)
    {
        long baseAt = fullAt > 0 ? fullAt : createdAt;
        return baseAt > 0 && nowMs - baseAt > ROOM_LIFETIME_MS;
    }

    // 미니언 목록 "Key:표시레벨,…" ↔ (key, lv) — 같은 종·레벨끼리 묶지 않고 칸마다 (보이는 순서 = 레벨 높은 순)
    public static string FormatMinions(IEnumerable<(string key, int lv)> minions) =>
        string.Join(",", minions.OrderByDescending(m => m.lv).Select(m => $"{m.key}:{m.lv}"));

    public static List<(string key, int lv)> ParseMinions(string text)
    {
        var list = new List<(string, int)>();
        if (string.IsNullOrEmpty(text))
            return list;
        foreach (string part in text.Split(','))
        {
            string[] kv = part.Split(':');
            if (kv.Length == 2 && int.TryParse(kv[1], out int lv) && kv[0].Length > 0)
                list.Add((kv[0], lv));
        }
        return list;
    }

    // 순위: 도착한 사람(빠른 순) → 아직 하는 사람·탈락한 사람(높은 wave 순, 같으면 진행 중 > 늦게 탈락 > 먼저 탈락) (씬 없이 계산)
    public static List<MultiPlayerState> Rank(IEnumerable<MultiPlayerState> players) =>
        players.OrderByDescending(p => p.HasReached)
            .ThenBy(p => p.HasReached ? p.reachedAt : 0)
            .ThenByDescending(p => p.wave)
            .ThenBy(p => p.IsOut)
            .ThenByDescending(p => p.IsOut ? p.endedAt : 0)
            .ThenBy(p => p.joinedAt)
            .ToList();

    // 방장 고르기 (씬 없이 계산): online 중 joinedAt이 가장 이른 사람 (모두 오프라인이면 가장 이른 사람)
    public static string PickHost(IEnumerable<MultiPlayerState> players)
    {
        var list = players.Where(p => p != null && !p.ai).OrderBy(p => p.joinedAt).ThenBy(p => p.uid).ToList();
        MultiPlayerState host = list.FirstOrDefault(p => p.online) ?? list.FirstOrDefault();
        return host?.uid;
    }

    // ---- (2026-11-27) AI 봇: 방이 AI_FILL_WAIT_SEC(300초) 안에 안 차면 방장이 남은 자리를 플레이 기록 재생 봇으로 채움 (모든 방 같음)
    public const float AI_FILL_WAIT_SEC = 300f;
    public const float AI_TICK_SEC = 2f;          // 방장이 봇 상황을 올리는 간격
    public const string AI_UID_PREFIX = "ai_";
    static readonly string[] AI_NAME_HEAD = { "슬라임", "꼬마용사", "마왕", "도깨비", "불꽃", "얼음", "번개", "달빛", "별빛", "초코", "말랑", "킹" };
    public static string RandomAiNickname(System.Random rng) => $"{AI_NAME_HEAD[rng.Next(AI_NAME_HEAD.Length)]}{rng.Next(10, 1000)}";
    public static bool IsAiUid(string uid) => uid != null && uid.StartsWith(AI_UID_PREFIX);

    // 기록 재생: 판 시작부터 elapsed초일 때의 장면 (그 시각까지 지난 마지막 브레이크타임, 아직 없으면 첫 장면) (씬 없이 계산)
    public static PlayRecord.Frame FrameAt(PlayRecord record, float elapsed)
    {
        if (record == null || record.frames == null || record.frames.Count == 0)
            return null;
        PlayRecord.Frame found = record.frames[0];
        foreach (PlayRecord.Frame f in record.frames)
        {
            if (f.t > elapsed)
                break;
            found = f;
        }
        return found;
    }

    // AI 봇으로 쓸 기록 고르기: 스테이지 1 보통(기록도 이것만 남김) 먼저, 장면 3장 이상, 랜덤 (씬 없이 계산)
    public static List<PlayRecordSummary> PickAiRecords(IEnumerable<PlayRecordSummary> all, int count, System.Random rng)
    {
        var ok = all.Where(s => s != null && s.frameCount >= 3).ToList();
        var same = ok.Where(s => s.stageOrder == MULTI_STAGE_ORDER && s.difficulty == MULTI_DIFFICULTY).OrderBy(_ => rng.Next()).ToList();
        var other = ok.Except(same).OrderBy(_ => rng.Next()).ToList();
        return same.Concat(other).Take(count).ToList();
    }

    public const int CODE_LENGTH = 7;
    public static bool IsValidCode(string code) =>
        !string.IsNullOrEmpty(code) && code.Length == CODE_LENGTH && code.All(char.IsDigit) && code[0] != '0';

    public static void Reset()
    {
        IsActive = false;
        RoomId = null;
        Room = null;
        CanStart = false;
    }
}

// 유저 한 명 (방 players/{uid})
public class MultiPlayerState
{
    public string uid;
    public string nickname;
    public bool ready;
    public long joinedAt;
    public bool online = true; // (2026-11-27) 앱이 꺼지면 false (onDisconnect) → 방장은 online인 사람 중 가장 먼저 들어온 사람
    public bool ai;            // (2026-11-27) 플레이 기록을 재생하는 AI 봇 (방장이 움직임, 방장이 될 수 없음)
    public int aiSlot = -1;    // 재생하는 기록 칸 (PlayRecordStore)
    // ---- 판 상황 (MultiStatusReporter가 브레이크타임·wave 시작마다 씀)
    public int wave;          // 지금 wave (브레이크타임이면 다음 wave)
    public int kingLv;
    public string heroKey;    // HeroData key (없으면 빈칸)
    public string minions;    // "Key:표시레벨,..." (MultiGame.FormatMinions)
    public string board;      // 판 배치 JSON (MultiBoard, 보기 화면)
    public long reachedAt;    // 웨이브 50 보스 처치 서버 시각 (0 = 아직)
    public bool ended;        // 내 판이 끝남 (도착 또는 탈락)
    public long endedAt;
    public bool HasReached => reachedAt > 0;
    public bool IsOut => ended && !HasReached; // 탈락
}

// 방 (multiRooms/{roomIndex})
public class MultiRoomState
{
    public string roomId;
    public long createdAt;
    public long fullAtMs;               // 8명이 찬 서버 시각 (0 = 아직)
    public string winnerUid;            // 가장 먼저 도착한 사람 (트랜잭션)
    public string code;                 // (2026-11-27) 7자리 방 코드 (codes/{code} = 방 번호, 친구가 코드로 입장)
    public bool isPrivate;              // 방만들기로 만든 방 (빠른 매칭 대기열에 안 들어감)
    public long startAtMs;              // 방장이 정한 시작 시각 (0 = 아직, 그 전엔 계산값)
    public List<MultiPlayerState> players = new List<MultiPlayerState>();

    public int Count => players.Count;
    public bool IsFull => players.Count >= MultiGame.ROOM_SIZE && fullAtMs > 0;
    public int ReadyCount => players.Count(p => p.ready);
    // 시작 시각: 방장이 쓴 값이 있으면 그것, 없으면 계산 (방장이 Ready 수가 바뀔 때마다 다시 씀)
    public long StartAtMs => startAtMs > 0 ? startAtMs : MultiGame.GetStartAtMs(fullAtMs, ReadyCount);
    // (2026-11-27) 방장 = 접속 중인 사람 중 가장 먼저 들어온 사람 (방장이 나가면 그다음 사람)
    public string HostUid => MultiGame.PickHost(players);
    public bool IsReady(string uid) => players.Any(p => p.uid == uid && p.ready);
    public MultiPlayerState Get(string uid) => players.FirstOrDefault(p => p.uid == uid);
}

// 보기 화면용 판 배치 (JSON 문자열로 저장)
[System.Serializable]
public class MultiBoard
{
    [System.Serializable]
    public class Cell
    {
        public int x, y;
        public string minion;   // 미니언 key (없으면 빈칸)
        public int lv;          // 표시 레벨 (머지 레벨 + 1)
        public bool evolved;
        public string rune;     // 룬 key
        public int runeLv;
        public bool king;
        public bool blocked;
    }
    public List<Cell> cells = new List<Cell>();
}
