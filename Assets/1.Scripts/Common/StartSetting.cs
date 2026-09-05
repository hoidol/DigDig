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
            "EquipmentManager",
            "CharacterManager",
            "SlimeManager",
            "SoundManager",
//공용
            "InAppPurchaseManager",
            "AdManager",

//UI --------
            "FadeCanvas",
            "BlockCanvas",
            "ToastCanvas",
            "YesOrNoCanvas"

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
        //createSingleton(PrefabPath_FirebaseMGR);
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