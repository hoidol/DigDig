using System.Collections;
using UnityEngine;

// (2026-11-27) 로컬(싱글) 판 이어하기: 싱글 판의 브레이크타임이 시작될 때마다 판 상태를 파일로 저장 (GameSnapshot)
// → 멀티 대기 중 하던 싱글 판에서 매칭되면 멀티로 넘어가고, 멀티가 끝난 뒤 그 브레이크타임부터 다시 (로비 "이어하기" 버튼 등 → RequestResume → InGame)
// 웨이브 도중에 넘어가면 그 웨이브 진행분은 잃고 마지막 브레이크타임으로 돌아감
// 저장 삭제 = 싱글 판이 끝남(결과 화면). 저장 파일 = SaveManager "LocalGameSave" (에디터 Assets/LocalGameSave.json, git 제외)
public static class LocalGameSave
{
    public const string FILE_NAME = "LocalGameSave";

    [System.Serializable]
    public class Data
    {
        public int stageOrder;
        public DifficultyType difficulty;
        public string savedAt;
        public GameSnapshot snapshot;
    }

    // 다음 InGame(싱글)이 시작되면 저장본으로 복구 (GameManager.StartGame 뒤 TryResumeAfterStart)
    public static bool PendingResume { get; private set; }

    static Data cached;
    static bool loaded;

    public static Data Load()
    {
        if (!loaded)
        {
            cached = SaveManager.LoadData<Data>(FILE_NAME);
            if (cached != null && cached.snapshot == null)
                cached = null;
            loaded = true;
        }
        return cached;
    }

    public static bool HasSave => Load() != null;

    public static void Save(GameSnapshot snapshot)
    {
        if (snapshot == null)
            return;
        cached = new Data
        {
            stageOrder = UserDataManager.stageOrder,
            difficulty = UserDataManager.difficultyType,
            savedAt = TimeManager.NowToString(),
            snapshot = snapshot,
        };
        loaded = true;
        SaveManager.SaveData(FILE_NAME, cached);
    }

    public static void Clear()
    {
        cached = null;
        loaded = true;
        PendingResume = false;
        SaveManager.SaveData(FILE_NAME, new Data()); // snapshot null = 저장 없음
    }

    // ---- 싱글 판: 브레이크타임마다 저장 (GameManager.Start, 멀티·튜토리얼이 아닐 때)
    static MonoBehaviour runner;

    public static void Begin(MonoBehaviour owner)
    {
        runner = owner;
        GameEventBus.Unsubscribe<BreakTimeStartEvent>(OnBreakTimeStart);
        GameEventBus.Subscribe<BreakTimeStartEvent>(OnBreakTimeStart);
    }

    public static void End()
    {
        GameEventBus.Unsubscribe<BreakTimeStartEvent>(OnBreakTimeStart);
        runner = null;
    }

    static void OnBreakTimeStart(BreakTimeStartEvent e)
    {
        if (runner == null || GameSnapshot.IsRestoring)
            return;
        runner.StartCoroutine(CaptureNextFrame(e.nextWaveIdx)); // 카드 지급·주사위 코인 초기화가 끝난 다음 프레임
    }

    static IEnumerator CaptureNextFrame(int nextWaveIdx)
    {
        yield return null;
        if (GameSnapshot.IsRestoring || MultiGame.IsActive)
            yield break;
        Save(GameSnapshot.Capture(nextWaveIdx));
    }

    // ---- 이어하기
    // 저장한 스테이지로 맞추고 다음 InGame 시작 때 복구. 저장이 없으면 false
    public static bool RequestResume()
    {
        Data data = Load();
        if (data == null)
            return false;
        UserDataManager.stageOrder = data.stageOrder;
        UserDataManager.difficultyType = data.difficulty;
        PendingResume = true;
        return true;
    }

    // GameManager.StartGame 직후 (싱글). 복구했으면 true
    public static bool TryResumeAfterStart()
    {
        if (!PendingResume || MultiGame.IsActive)
            return false;
        PendingResume = false;
        Data data = Load();
        if (data == null)
            return false;
        bool ok = data.snapshot.Apply();
        Debug.Log($"[LocalGameSave] 이어하기 Wave {data.snapshot.nextWaveIdx + 1} ({(ok ? "성공" : "실패")})");
        return ok;
    }
}
