using UnityEngine;

public class GameSetting
{
   public const int MAX_EQUIPT_BULLET_COUNT = 5;
   public const int MAX_MF_GROUP_POINT = 5; //MemoryFragment
   public static string FIRST_STAGE_KEY = "0";
   public static CharacterName INIT_CHARACTER_NAME = CharacterName.Lucky;
   public static string[] INIT_SLIME_KEYS = { "Flame", "Bounce", "Wacky", "Orbit", "Pierce" };
   public static int SLIME_SLOT_COUNT = 5;

   public const int WAVE_COUNT = 7;
   public const float BREAK_TIME = 20;
   public const float WAVE_TIME = 40;

   public const float WAVE_INCREASE_TIME = 5;
   public const float MIX_WAVE_TIME = 120;

   public const int INIT_ITEM_PRICE = 5;
   public const int INCREASE_ITEM_PRICE = 3;
   public const int INIT_SPAWN_PRICE = 5;
   public const int INCREASE_SPAWN_PRICE = 5;



   public const int MIN_SLIME_SLOT_COUNT = 6;
   public const int MAX_SLIME_SLOT_COUNT = 9;
   public const int LEVEL_TO_GROWUP2 = 2;//0,1,2 되어야됌



}
