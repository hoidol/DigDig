using UnityEngine;

public class GameSetting
{
   public const int MAX_EQUIPT_BULLET_COUNT = 5;
   public const int MAX_MF_GROUP_POINT = 5; //MemoryFragment
   public static string FIRST_STAGE_KEY = "0";
   public static CharacterName INIT_CHARACTER_NAME = CharacterName.Lucky;
   public static string[] INIT_BULLE_KEYS = new string[]
   {
      "Pierce","Flame","Giant","Thunder","Iron"
   };
   public const int DAY_COUNT = 7;
   public const float DAY_TIME = 40;
   public const float DAY_INCREASE_TIME = 3;
   public const float MIX_DAY_TIME = 70;
   public const float NIGHT_TIME = 40;
   public const float NIGHT_INCREASE_TIME = 5;
   public const float MIX_NIGHT_TIME = 120;
   public const int INIT_ITEM_PRICE = 5;
   public const int INCREASE_ITEM_PRICE = 3;
   public const int INIT_SPAWN_PRICE = 5;
   public const int INCREASE_SPAWN_PRICE = 1;
   public const int MAX_MiniMe_COUNT = 5;

}
