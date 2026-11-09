//#define TEST_MODE
using UnityEngine;
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
        string[] singletonTargets =
        {
            "ItemManager",
            "BulletManager",
            "UserManager",
            "StageManager",
            "EnemyManager",
            "RelicManager",
            "KingSlimeManager",
            "SlimeManager",
            "SoundManager",
            "AchievementManager",
            "EnergeManager",
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

#if !PLATFORM_STANDALONE_WIN
        // 프리팹 없이 생성 (설정할 인스펙터 값 없음)
        if (FirebaseManager.Instance == null)
            new GameObject(nameof(FirebaseManager)).AddComponent<FirebaseManager>();
#endif

    }


    // Functions
    private static void createSingleton(string path)
    {
        var pb = Resources.Load<GameObject>(path);
        var go = GameObject.Instantiate(pb);
        GameObject.DontDestroyOnLoad(go);
    }
}