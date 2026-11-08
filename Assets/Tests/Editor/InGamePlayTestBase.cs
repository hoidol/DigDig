using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

// InGame 씬을 실제로 플레이하는 테스트 공용.
// EditMode 테스트에서 EnterPlayMode로 들어간다(PlayMode 전용 asmdef 없이 게임 코드 참조).
// 주의: EnterPlayMode / ExitPlayMode 시 도메인 리로드로 테스트가 처음부터 다시 실행되므로, 그 앞의 코드는 여러 번 실행돼도 괜찮아야 한다.
//
// 주의: 도메인 리로드 후 테스트 이터레이터는 직렬화로 복원되는데, 람다가 캡처한 지역 변수(클로저)는 복원되지 않아 null이 된다.
//       → 플레이 모드 진입 뒤 본문은 별도 이터레이터(Body)로 분리한다 (리로드 후 새로 만들어짐).
// 사용법:
//     if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
//     yield return WaitGameStart();
//     yield return XxxBody();
public abstract class InGamePlayTestBase
{
    const string SCENE_PATH = "Assets/4.Scenes/InGame.unity";
    const float START_TIMEOUT = 60f; // 실시간(초)
    const string BACKUP_DIR = "Temp/PlayTestSaveBackup";

    // 테스트 환경에서 항상 나는 에러 (에디터용 Firebase 네이티브 라이브러리 없음)
    static readonly string[] IGNORE_ERRORS =
    {
        "FirebaseCppApp",
        "[Firebase]",
    };

    protected readonly List<string> errors = new List<string>();
    protected string current = ""; // 에러 메시지 앞에 붙는 현재 진행 중인 대상

    void OnLog(string msg, string stackTrace, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)
            return;
        if (IGNORE_ERRORS.Any(msg.Contains))
            return;
        string where = stackTrace.Split('\n').FirstOrDefault(l => l.Contains("1.Scripts")) ?? "";
        errors.Add($"[{current}] {type}: {msg.Split('\n')[0]} {where}");
    }

    // 에디터에서는 저장 파일이 Assets/User*.json (SaveManager). 플레이 중 업적 등이 저장되므로 백업 후 복원
    static string[] SaveFiles() => Directory.GetFiles("Assets", "User*.json");

    // 플레이 중 바뀌는 PlayerPrefs 문자열/정수 키 (일일 부활 횟수 등). 없던 키는 복원 시 삭제
    static readonly string[] PREF_KEYS =
    {
        FailCanvas.DAILY_RESURRECTION_COUNT_KEY,
        FailCanvas.DAILY_RESURRECTION_DATE_KEY,
    };
    const string PREFS_BACKUP_FILE = "PlayerPrefs.txt";

    static void BackupSaves()
    {
        if (Directory.Exists(BACKUP_DIR)) // 이전 테스트가 복원 못 하고 끝났으면 그 백업(더 깨끗한 쪽)을 유지
            return;
        Directory.CreateDirectory(BACKUP_DIR);
        foreach (string file in SaveFiles())
            File.Copy(file, Path.Combine(BACKUP_DIR, Path.GetFileName(file)));

        // 키 \t 있음(1/0) \t 문자열 \t 정수
        var lines = PREF_KEYS.Select(k => $"{k}\t{(PlayerPrefs.HasKey(k) ? 1 : 0)}\t{PlayerPrefs.GetString(k, "")}\t{PlayerPrefs.GetInt(k, 0)}");
        File.WriteAllLines(Path.Combine(BACKUP_DIR, PREFS_BACKUP_FILE), lines);
    }

    static void RestorePrefs(string path)
    {
        foreach (string line in File.ReadAllLines(path))
        {
            string[] v = line.Split('\t');
            string key = v[0];
            PlayerPrefs.DeleteKey(key);
            if (v[1] != "1")
                continue;
            if (!string.IsNullOrEmpty(v[2]))
                PlayerPrefs.SetString(key, v[2]);
            else
                PlayerPrefs.SetInt(key, int.Parse(v[3]));
        }
        PlayerPrefs.Save();
    }

    static void RestoreSaves()
    {
        if (!Directory.Exists(BACKUP_DIR))
            return;
        string prefsFile = Path.Combine(BACKUP_DIR, PREFS_BACKUP_FILE);
        if (File.Exists(prefsFile))
        {
            RestorePrefs(prefsFile);
            File.Delete(prefsFile);
        }
        var backups = Directory.GetFiles(BACKUP_DIR).Select(Path.GetFileName).ToHashSet();
        foreach (string file in SaveFiles())
        {
            if (!backups.Contains(Path.GetFileName(file))) // 테스트 중 새로 생긴 저장 파일
            {
                File.Delete(file);
                if (File.Exists(file + ".meta"))
                    File.Delete(file + ".meta");
            }
        }
        foreach (string name in backups)
            File.Copy(Path.Combine(BACKUP_DIR, name), Path.Combine("Assets", name), true);
        Directory.Delete(BACKUP_DIR, true);
        AssetDatabase.Refresh();
    }

    protected const string LOBBY_SCENE_PATH = "Assets/4.Scenes/Lobby.unity";

    protected static void BeforeEnterPlayMode(string scenePath = SCENE_PATH)
    {
        BackupSaves();
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
    }

    // 로비 시작 대기 (StartSetting이 공용 싱글톤 생성)
    protected IEnumerator WaitLobbyStart()
    {
        SkipPayments(true);
        LogAssert.ignoreFailingMessages = true;
        Application.logMessageReceived -= OnLog;
        Application.logMessageReceived += OnLog;

        double timeout = EditorApplication.timeSinceStartup + START_TIMEOUT;
        while (UserDataManager.Instance == null || BlockCanvas.Instance == null || ProductManager.Instance == null
            || SlimeManager.Instance == null || !SlimeManager.Instance.LoadTask.Status.IsCompleted()
            || EquipmentManager.Instance == null || !EquipmentManager.Instance.LoadTask.Status.IsCompleted())
        {
            Assert.Less(EditorApplication.timeSinceStartup, timeout, "로비가 시작되지 않음");
            Unpause();
            yield return null;
        }
        yield return WaitFrames(5);
    }

    // 게임 시작 대기 (데이터 로드 → FadeIn → StartGame). 에러는 직접 모아서 판단 (Firebase 같은 환경 에러 제외)
    // 테스트 중엔 광고·인앱 결제를 띄우지 않고 바로 성공
    static void SkipPayments(bool skip)
    {
        AdManager.SkipAdForTest = skip;
        ProductManager.SkipInAppPurchaseForTest = skip;
    }

    protected IEnumerator WaitGameStart()
    {
        SkipPayments(true);
        LogAssert.ignoreFailingMessages = true;
        Application.logMessageReceived -= OnLog;
        Application.logMessageReceived += OnLog;

        double timeout = EditorApplication.timeSinceStartup + START_TIMEOUT;
        while (GameManager.Instance == null || !GameManager.Instance.isPlaying)
        {
            Assert.Less(EditorApplication.timeSinceStartup, timeout, "게임이 시작되지 않음");
            Unpause();
            yield return null;
        }
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Application.logMessageReceived -= OnLog;
        Time.timeScale = 1f;
        SkipPayments(false);
        if (Application.isPlaying)
            yield return new ExitPlayMode();
        RestoreSaves();
    }

    // 콘솔 Error Pause로 멈추면 풀어 준다
    static void Unpause()
    {
        if (EditorApplication.isPaused)
            EditorApplication.isPaused = false;
    }

    // 실시간 기준 대기. 킹이 죽으면 EndGame에서 저장되므로 죽지 않게 체력을 채운다
    protected static IEnumerator WaitRealtime(float seconds)
    {
        double end = EditorApplication.timeSinceStartup + seconds;
        do
        {
            Unpause();
            if (KingSlime.Instance != null)
                KingSlime.Instance.AddHp(100000, false);
            yield return null;
        } while (EditorApplication.timeSinceStartup < end);
    }

    protected static IEnumerator WaitFrames(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Unpause();
            yield return null;
        }
    }

    protected void AssertNoErrors(string title = "에러")
    {
        Assert.IsEmpty(errors, $"{title}\n{string.Join("\n", errors.Distinct())}");
    }

    // 비어 있고 아이템도 없는 타일 (except 제외)
    protected static Tile GetFreeTile(params Tile[] except)
    {
        return TileManager.Instance.tiles.FirstOrDefault(t => t.IsEmpty && t.CanAddTileItem && !except.Contains(t));
    }

    protected static Slime SpawnSlime(string key, Tile tile, int lv = 0)
    {
        Slime slime = KingSlime.Instance.AddSlime(key, lv);
        tile.SetTileObject(slime);
        return slime;
    }
}
