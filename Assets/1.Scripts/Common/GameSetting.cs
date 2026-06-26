using UnityEngine;

[CreateAssetMenu(fileName = "GameSetting", menuName = "Settings/GameSetting", order = 0)]
public class GameSetting : ScriptableObject
{
    public const int MAX_EQUIPT_BULLET_COUNT = 5;
    public const int MAX_MF_GROUP_POINT = 5; //MemoryFragment
    public static string FIRST_STAGE_KEY ="Gateway1";
   public static string[] INIT_BULLE_KEYS = new string[]
   {
      "Pierce","Flame","Giant","Thunder","Iron"
   };
    // private static GameSetting instance;

    // // 어디서든 쉽게 접근 가능한 인스턴스
    // public static GameSetting Instance
    // {
    //     get
    //     {
    //         if (instance == null)
    //         {
    //             // Resources 폴더에서 자동으로 BuildSetting.asset을 로드
    //             instance = Resources.Load<GameSetting>("GameSetting");
    //             if (instance == null)
    //             {
    //                 Debug.LogError("BuildSetting.asset이 Resources 폴더에 없습니다!");
    //             }
    //         }
    //         return instance;
    //     }
    // }

    // [Header("Build Settings")]
    // public bool isTestMode = false;
}
