using System.Collections;
using UnityEngine;

// (2026-11-27) 플레이 기록 (스테이지 1 보통만, 싱글·멀티): 브레이크타임이 시작될 때마다(카드 지급 뒤 다음 프레임) 지금 상태 한 장(PlayRecord.Frame)
// 장면마다 그 브레이크타임이 시작된 서버 시각(ServerClock)도 남김
// 판이 끝나면(결과 화면·멀티 결과) 로컬 백업(PlayRecordBackup) → 서버(공용 DB, 최대 300판 돌려쓰기)
// 싱글 진행 중 기록은 파일(PlayRecordCurrent)에도 남김 → 앱이 꺼지는 등으로 끝내지 못한 판은 다음 판을 시작할 때 abandoned로 끝냄
public static class PlayRecorder
{
    public const string CURRENT_FILE = "PlayRecordCurrent";

    static PlayRecord current;
    static MonoBehaviour runner;

    public static PlayRecord Current => current;

    // 남기는 판 = 스테이지 1 보통 (멀티도 이 맵·난이도) (씬 없이 계산)
    public static bool ShouldRecord(int stageOrder, DifficultyType difficulty) =>
        stageOrder == StageData.FIRST_STAGE_ORDER && difficulty == DifficultyType.Normal;

    // GameManager: StartGame 뒤
    public static void Begin(MonoBehaviour owner)
    {
        End();
        if (!ShouldRecord(UserDataManager.stageOrder, UserDataManager.difficultyType))
            return; // 다른 스테이지·난이도는 안 남김
        runner = owner;
        bool multi = MultiGame.IsActive;
        PlayRecord saved = multi ? null : LoadCurrent();
        if (saved != null && saved.frames.Count > 0)
        {
            saved.abandoned = true; // 끝내지 못한 이전 판 (앱 종료 등)
            FinishInternal(saved, false);
        }
        current = new PlayRecord
        {
            id = System.Guid.NewGuid().ToString("N"),
            mode = multi ? PlayRecord.MODE_MULTI : PlayRecord.MODE_SINGLE,
            stageOrder = UserDataManager.stageOrder,
            difficulty = UserDataManager.difficultyType,
            startedAt = ServerClock.NowMs,
        };
        GameEventBus.Subscribe<BreakTimeStartEvent>(OnBreakTimeStart);
        if (GameManager.Instance != null && GameManager.Instance.isBreakTime)
            runner.StartCoroutine(CaptureNextFrame()); // 이미 시작한 브레이크타임 (wave 1)
    }

    // 씬이 바뀌어 기록을 멈춤 (판은 안 끝남 — 싱글은 파일에 남아 있음)
    public static void End()
    {
        GameEventBus.Unsubscribe<BreakTimeStartEvent>(OnBreakTimeStart);
        runner = null;
        current = null;
    }

    static void OnBreakTimeStart(BreakTimeStartEvent e)
    {
        if (runner != null && !GameSnapshot.IsRestoring)
            runner.StartCoroutine(CaptureNextFrame());
    }

    static IEnumerator CaptureNextFrame()
    {
        yield return null;
        Capture();
    }

    public static void Capture()
    {
        if (current == null || GameManager.Instance == null)
            return;
        MultiStatus s = MultiInGameSession.BuildStatus();
        KingSlime king = KingSlime.Instance;
        var frame = new PlayRecord.Frame
        {
            t = GameResultStats.PlayTime,
            at = ServerClock.NowMs,
            wave = s.wave,
            kingLv = s.kingLv,
            heroKey = s.heroKey,
            hp = king != null && king.MaxHp > 0 ? Mathf.Clamp01(king.CurHp / king.MaxHp) : 1f,
            diceCoin = king != null ? king.diceCoin : 0,
            minions = s.minions,
            board = s.board,
        };
        var frames = current.frames;
        if (frames.Count > 0 && frames[frames.Count - 1].wave >= frame.wave)
            frames.RemoveAll(f => f.wave >= frame.wave); // 같은 브레이크타임(복구·되돌리기)은 새 값으로
        frames.Add(frame);
        current.playTime = frame.t;
        current.finalWave = frame.wave;
        if (!current.IsMulti)
            SaveManager.SaveData(CURRENT_FILE, current);
    }

    // 판 끝 (싱글 결과 화면 / 멀티 결과). 로컬 백업 → 올리기
    public static void Finish(bool cleared)
    {
        if (current == null)
            return;
        PlayRecord r = current;
        current = null;
        GameManager gm = GameManager.Instance;
        if (gm != null)
            r.finalWave = Mathf.Max(r.finalWave, gm.waveIdx + 1);
        r.playTime = Mathf.Max(r.playTime, GameResultStats.PlayTime);
        FinishInternal(r, cleared);
    }

    static void FinishInternal(PlayRecord r, bool cleared)
    {
        r.cleared = cleared;
        if (!r.IsMulti)
            SaveManager.SaveData(CURRENT_FILE, new PlayRecord { id = null }); // 진행 중 기록 비움
        if (r.frames.Count == 0)
            return; // 브레이크타임을 한 번도 못 지난 판은 버림
        PlayRecordBackup.Add(r);
        Cysharp.Threading.Tasks.UniTaskExtensions.Forget(PlayRecordBackup.UploadPendingAsync());
        Debug.Log($"[PlayRecord] 판 끝 {r.mode} W{r.finalWave} {(r.cleared ? "클리어" : r.abandoned ? "버림" : "")} 장면 {r.frames.Count}");
    }

    static PlayRecord LoadCurrent()
    {
        PlayRecord r = SaveManager.LoadData<PlayRecord>(CURRENT_FILE);
        return r != null && !string.IsNullOrEmpty(r.id) ? r : null;
    }
}
