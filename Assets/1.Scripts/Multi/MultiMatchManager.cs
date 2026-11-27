using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

// (2026-11-27) 멀티 매칭 (씬을 넘어 살아 있음, 처음 쓸 때 Instance 생성)
// 대기열 참여 → 방 구독 → 오른쪽 위 N/8 (Resources/UI/MultiMatchStatus 프리팹, 메뉴 Tools/Multi/Create MultiMatchStatus Prefab)
// → 8명이 차면 멀티 InGame(+ MultiGameUI 씬 추가)으로. 싱글 판을 하던 중이면 그 판은 마지막 브레이크타임 저장본(LocalGameSave)으로 나중에 이어하기
// 서버 = Firebase (에디터·테스트는 FakeMultiBackend: 봇이 채움)
// (2026-11-27) 들어가는 방법 3가지: 빠른 매칭(Join) · 방만들기(CreateRoom, 코드로만 입장) · 코드입력(JoinByCode)
// 방장 = 접속 중 가장 먼저 들어온 사람 (나가면 다음 사람). 방장이 하는 일: 8명이 차면 fullAt · Ready 수마다 startAt · 오래된 방 정리 (OnRoomChanged)
public class MultiMatchManager : MonoBehaviour
{
    public const string STATUS_PREFAB_PATH = "UI/MultiMatchStatus";
    public const string UI_SCENE_NAME = "MultiGameUI";
    public const string INGAME_SCENE_NAME = "InGame";
    public const string LOBBY_SCENE_NAME = "Lobby";

    public enum MatchState { Idle, Joining, Waiting, Starting, InGame } // Starting = 방이 참 → "5초 후 시작합니다"
    public enum JoinMode { Quick, Create, Code }

    static MultiMatchManager instance;
    public static MultiMatchManager Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject(nameof(MultiMatchManager));
                DontDestroyOnLoad(go);
                instance = go.AddComponent<MultiMatchManager>();
            }
            return instance;
        }
    }
    public static bool Exists => instance != null;

    // 테스트·에디터: true면 가짜 서버 (기본 = 에디터이거나 Firebase가 없으면 가짜)
    public static bool? ForceFakeBackend;

    public MatchState State { get; private set; }
    public MultiRoomState Room { get; private set; }
    public IMultiBackend Backend { get; private set; }
    public event Action<MultiRoomState> RoomChanged;

    MultiMatchStatusPanel statusPanel;
    int prevStageOrder;
    DifficultyType prevDifficulty;

    public bool IsWaiting => State == MatchState.Joining || State == MatchState.Waiting || State == MatchState.Starting;
    public bool CanCancel => State == MatchState.Joining || State == MatchState.Waiting;
    public bool IsHost => Room != null && Backend != null && Room.HostUid == Backend.MyUid;
    public string RoomCode => Room?.code;
    string cleanedRoomId;      // 방장으로 오래된 방 정리를 한 방
    readonly MultiAiDirector ai = new MultiAiDirector(); // (2026-11-27) 플레이 기록 재생 AI 봇 (방장)
    // 테스트: AI 봇으로 채우기 전 기다리는 시간 (null = MultiGame.AI_FILL_WAIT_SEC)
    public static float? AiFillWaitForTest;
    long lastStartAtWritten;

    IMultiBackend CreateBackend()
    {
        bool fake = ForceFakeBackend ?? (Application.isEditor || FirebaseManager.Instance == null);
        return fake ? new FakeMultiBackend() : new FirebaseMultiBackend();
    }

    // ---- (2026-11-27) 오래된 방 정리: 로비에 들어올 때마다 누구든 (방장이 될 때도). 앱을 켠 동안 CLEANUP_INTERVAL_SEC마다 한 번
    // 서버 자동 삭제가 없어서 누군가 앱을 켜야 지워짐 (아무도 안 켜는 동안은 남음, 정확한 시점이 필요하면 Cloud Functions 예약 작업)
    public const float CLEANUP_INTERVAL_SEC = 600f;
    static float lastCleanupAt = -CLEANUP_INTERVAL_SEC;

    public static void CleanupOldRoomsOnLobby()
    {
        if (UnityEngine.Time.realtimeSinceStartup - lastCleanupAt < CLEANUP_INTERVAL_SEC)
            return;
        lastCleanupAt = UnityEngine.Time.realtimeSinceStartup;
        MultiMatchManager mm = Instance;
        mm.Backend ??= mm.CreateBackend();
        mm.Backend.CleanupOldRoomsAsync().Forget();
    }

    // ---- 대기열
    public UniTask<bool> Join() => Join(JoinMode.Quick, null);
    public UniTask<bool> CreateRoom() => Join(JoinMode.Create, null);
    public UniTask<bool> JoinByCode(string code) => Join(JoinMode.Code, code);

    public async UniTask<bool> Join(JoinMode mode, string code)
    {
        if (State != MatchState.Idle)
            return false;
        State = MatchState.Joining;
        Backend ??= CreateBackend();
        EnsureStatusPanel();
        UpdateStatus();
        string nickname = RankingManager.GetMyNickname(Backend.MyUid);
        MultiJoinResult result = mode == JoinMode.Create ? await Backend.CreateRoomAsync(nickname)
            : mode == JoinMode.Code ? await Backend.JoinByCodeAsync(nickname, code)
            : await Backend.JoinAsync(nickname);
        if (!result.Ok)
        {
            State = MatchState.Idle;
            UpdateStatus();
            ToastCanvas.Toast(TranslateManager.GetText(result.error ?? "Multi_JoinFailed"));
            return false;
        }
        if (State != MatchState.Joining) // 기다리는 동안 취소
        {
            await Backend.LeaveAsync(result.roomId);
            return false;
        }
        State = MatchState.Waiting;
        MultiGame.RoomId = result.roomId;
        cleanedRoomId = null;
        lastStartAtWritten = 0;
        Backend.Listen(result.roomId, OnRoomChanged);
        UpdateStatus();
        return true;
    }

    public async UniTask Cancel()
    {
        if (!CanCancel)
            return; // 방이 찬 뒤("5초 후 시작")엔 취소 안 됨
        string roomId = MultiGame.RoomId;
        bool joining = State == MatchState.Joining;
        Backend?.StopListen();
        State = MatchState.Idle;
        Room = null;
        MultiGame.Reset();
        UpdateStatus();
        if (!joining && Backend != null)
            await Backend.LeaveAsync(roomId);
    }

    void OnRoomChanged(MultiRoomState room)
    {
        Room = room;
        MultiGame.Room = room;
        if (IsHost)
        {
            DoHostWork(room);
            room = Room; // 방장 일로 방이 바로 바뀌었을 수 있음 (가짜 서버). 안쪽 호출에서 이미 넘어갔으면 State = InGame
        }
        UpdateStatus();
        RoomChanged?.Invoke(room);
        if (State == MatchState.Waiting && room.IsFull)
            State = MatchState.Starting; // "5초 후 시작합니다" → Update에서 넘어감
    }

    // 방장 일 (방장이 바뀌면 새 방장이 이어서 함)
    void DoHostWork(MultiRoomState room)
    {
        string roomId = MultiGame.RoomId;
        if (cleanedRoomId != roomId)
        {
            cleanedRoomId = roomId;
            Backend.CleanupOldRoomsAsync().Forget();
        }
        if (room.fullAtMs <= 0 && room.players.Count >= MultiGame.ROOM_SIZE)
        {
            Backend.SetFullAtAsync(roomId).Forget();
            return;
        }
        if (room.fullAtMs > 0 && Backend.ServerNowMs < room.StartAtMs)
        {
            long startAt = MultiGame.GetStartAtMs(room.fullAtMs, room.ReadyCount);
            if (startAt != room.startAtMs && startAt != lastStartAtWritten)
            {
                lastStartAtWritten = startAt;
                Backend.SetStartAtAsync(roomId, startAt).Forget();
            }
        }
    }

    // ---- 멀티 판 시작/끝
    void EnterMultiGame()
    {
        State = MatchState.InGame;
        UpdateStatus();
        // 하던 싱글 판(InGame)은 마지막 브레이크타임 저장본이 이미 있음 (LocalGameSave)
        prevStageOrder = UserDataManager.stageOrder;
        prevDifficulty = UserDataManager.difficultyType;
        UserDataManager.stageOrder = MultiGame.MULTI_STAGE_ORDER;
        UserDataManager.difficultyType = MultiGame.MULTI_DIFFICULTY;
        MultiGame.IsActive = true;
        MultiGame.CanStart = false;
        Time.timeScale = 1f;
        SceneManager.sceneLoaded += OnSceneLoaded;
        FadeCanvs.Instance.FadeOutIn(null, () => SceneManager.LoadScene(INGAME_SCENE_NAME));
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != INGAME_SCENE_NAME || mode != LoadSceneMode.Single)
            return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnUiSceneLoaded;
        new GameObject(nameof(MultiInGameSession)).AddComponent<MultiInGameSession>(); // InGame 씬에 붙어 같이 사라짐
        if (Application.CanStreamedLevelBeLoaded(UI_SCENE_NAME))
            SceneManager.LoadScene(UI_SCENE_NAME, LoadSceneMode.Additive);
        else
        {
            Debug.LogWarning($"[Multi] {UI_SCENE_NAME} 씬이 빌드 설정에 없음 — 메뉴 Tools/Multi/Create MultiGameUI Scene · 카운트다운 없이 바로 시작");
            MultiGame.CanStart = true;
        }
    }

    public float AiFillWaitSec => AiFillWaitForTest ?? MultiGame.AI_FILL_WAIT_SEC;

    // (2026-11-27) 대기 표시 카운트다운 · 방이 차면 5초 뒤 멀티 판 · 방장: 300초 안에 안 차면 AI 봇으로 채움, 판 중엔 봇 상황 올림
    void Update()
    {
        MultiRoomState room = Room;
        if (room == null || Backend == null)
            return;
        if (State == MatchState.Waiting || State == MatchState.Starting)
            UpdateStatus(); // 남은 시간 글씨
        if (State == MatchState.Starting && Backend.ServerNowMs >= MultiGame.GetEnterAtMs(room.fullAtMs))
        {
            EnterMultiGame();
            return;
        }
        if (!IsHost)
            return;
        if (State == MatchState.Waiting && room.fullAtMs <= 0 && room.players.Count < MultiGame.ROOM_SIZE && room.createdAt > 0
            && MultiGame.GetAiFillRemain(room.createdAt, Backend.ServerNowMs, AiFillWaitSec) <= 0f)
            ai.FillAsync(Backend, room).Forget();
        else if (State == MatchState.InGame)
            ai.Tick(Backend, room, Time.unscaledDeltaTime);
    }

    // MultiGameUI 씬: 씬에서 꺼 둔 채 저장해도 Ready 창·다른 유저 목록은 켬 (꺼진 오브젝트는 스스로 못 켬)
    void OnUiSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != UI_SCENE_NAME)
            return;
        SceneManager.sceneLoaded -= OnUiSceneLoaded;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (MultiReadyPanel ready in root.GetComponentsInChildren<MultiReadyPanel>(true))
            {
                ready.gameObject.SetActive(true);
                if (ready.root != null)
                    ready.root.SetActive(true);
            }
            foreach (MultiPlayersPanel players in root.GetComponentsInChildren<MultiPlayersPanel>(true))
                players.gameObject.SetActive(true);
        }
    }

    // 멀티 판을 나감 → 로비 (다음 단계: 결과·전적). 싱글 저장본이 있으면 로비에서 이어하기
    public void ExitMultiGame()
    {
        if (Backend != null && !string.IsNullOrEmpty(MultiGame.RoomId))
            Backend.LeaveGameAsync(MultiGame.RoomId).Forget(); // online = false → 방장이면 다음 사람이 봇을 이어서 움직임
        Backend?.StopListen();
        State = MatchState.Idle;
        Room = null;
        MultiGame.Reset();
        UserDataManager.stageOrder = prevStageOrder;
        UserDataManager.difficultyType = prevDifficulty;
        UpdateStatus();
        Time.timeScale = 1f;
        FadeCanvs.Instance.FadeOutIn(null, () => SceneManager.LoadScene(LOBBY_SCENE_NAME));
    }

    public void SetReady()
    {
        if (State == MatchState.InGame && Backend != null && !string.IsNullOrEmpty(MultiGame.RoomId))
            Backend.SetReadyAsync(MultiGame.RoomId).Forget();
    }

    // ---- 판 진행 (MultiInGameSession)
    public void UpdateStatus(MultiStatus status)
    {
        if (State == MatchState.InGame && Backend != null && !string.IsNullOrEmpty(MultiGame.RoomId))
            Backend.UpdateStatusAsync(MultiGame.RoomId, status).Forget();
    }

    public async UniTask<bool> ReportReach()
    {
        if (Backend == null || string.IsNullOrEmpty(MultiGame.RoomId))
            return false;
        return await Backend.ReportReachAsync(MultiGame.RoomId);
    }

    public async UniTask ReportEnd()
    {
        if (Backend != null && !string.IsNullOrEmpty(MultiGame.RoomId))
            await Backend.ReportEndAsync(MultiGame.RoomId);
    }

    public long ServerNowMs => Backend != null ? Backend.ServerNowMs : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public string MyUid => Backend?.MyUid;

    // ---- 오른쪽 위 N/8 (대기 중에만)
    void EnsureStatusPanel()
    {
        if (statusPanel != null)
            return;
        MultiMatchStatusPanel prefab = Resources.Load<MultiMatchStatusPanel>(STATUS_PREFAB_PATH);
        if (prefab == null)
        {
            Debug.LogWarning($"[Multi] Resources/{STATUS_PREFAB_PATH} 프리팹 없음 — 메뉴 Tools/Multi/Create MultiMatchStatus Prefab");
            return;
        }
        statusPanel = Instantiate(prefab, transform);
    }

    void UpdateStatus()
    {
        if (statusPanel == null)
            return;
        statusPanel.gameObject.SetActive(IsWaiting);
        if (IsWaiting)
        {
            long now = ServerNowMs;
            statusPanel.Set(Room != null ? Room.Count : 1, MultiGame.ROOM_SIZE, RoomCode, IsHost);
            if (State == MatchState.Starting && Room != null)
                statusPanel.SetStarting(MultiGame.GetRemainSeconds(MultiGame.GetEnterAtMs(Room.fullAtMs), now));
            else
                statusPanel.SetWaiting(Room != null ? MultiGame.GetAiFillRemain(Room.createdAt, now, AiFillWaitSec) : AiFillWaitSec, ai.IsFilling);
        }
    }

    // 테스트: 가짜 서버에서 그 유저가 나감
    public void SimulateLeaveForTest(string uid) => (Backend as FakeMultiBackend)?.SimulateLeave(uid);

    // 테스트: 상태 초기화
    public static void ResetForTest()
    {
        if (instance != null)
        {
            instance.Backend?.StopListen();
            Destroy(instance.gameObject);
        }
        instance = null;
        MultiGame.Reset();
    }
}
