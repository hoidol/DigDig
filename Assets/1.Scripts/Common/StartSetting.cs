//#define TEST_MODE
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;



#if UNITY_EDITOR
using UnityEditor;
#endif
public class StartSetting
{
    // Definition


    // Methods
#if UNITY_EDITOR
    [InitializeOnLoadMethod]
#endif
    public static void InitializeOnLoadMethod()
    {

    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (IsToolScene())
            return;

        string[] singletonTargets =
        {
            "ItemManager",
            "BulletManager",
            "UserManager",
            "StageManager",
            "EnemyManager",
            "KingSlimeManager",
            "MinionManager",
            "SoundManager",
            "AchievementManager",
            "EnergeManager",
            "TutorialManager",
            //"FirebaseManager"
//공용
            // "InAppPurchaseManager",
            // "AdManager",

//UI --------
            "FadeCanvas",
            "BlockCanvas",
            "ToastCanvas",
            "YesOrNoCanvas",
            "RewardEffectCanvas"

        };

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        for (int i = 0; i < singletonTargets.Length; i++)
        {
            createSingleton($"Init/{singletonTargets[i]}");
        }


#if !UNITY_EDITOR
        InitializeOnLoadMethod();
#endif
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeAfterSceneLoad()
    {
        if (IsToolScene())
            return;

#if !PLATFORM_STANDALONE_WIN
        // 프리팹 없이 생성 (설정할 인스펙터 값 없음)
        if (FirebaseManager.Instance == null)
            new GameObject(nameof(FirebaseManager)).AddComponent<FirebaseManager>();
#endif

    }


    // Functions
    // 개발 도구 씬(EffectMaker: 이펙트 고르기)은 게임 매니저·유저 데이터·사운드를 만들지 않음
    const string EFFECT_MAKER_SCENE_NAME = "EffectMaker";
    public static bool IsToolScene()
    {
#if UNITY_EDITOR
        return SceneManager.GetActiveScene().name == EFFECT_MAKER_SCENE_NAME;
#else
        return false;
#endif
    }

    private static void createSingleton(string path)
    {
        var pb = Resources.Load<GameObject>(path);
        var go = GameObject.Instantiate(pb);
        GameObject.DontDestroyOnLoad(go);
    }
}