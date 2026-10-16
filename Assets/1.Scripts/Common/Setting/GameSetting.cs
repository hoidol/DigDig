using UnityEngine;

public class GameSetting
{
   public const int MAX_EQUIPT_BULLET_COUNT = 5;
   public const int MAX_MF_GROUP_POINT = 5; //MemoryFragment
   public static string FIRST_STAGE_KEY = "Normal_0";
   public static CharacterName INIT_CHARACTER_NAME = CharacterName.Lucky;
   public static string[] INIT_SLIME_KEYS = { "Flame", "Bounce", "Wacky", "Orbit", "Pierce" };
   public static int SLIME_SLOT_COUNT = 5;
   public static int PLAY_COST_ENERGE = 5;
   public static int MAX_ENERGE = 30;

   public const int WAVE_COUNT = 7;
   public const int BOSS_PHASE = 14;
   public const float BREAK_TIME = 25;
   public const float WAVE_TIME = 60;

   public const float WAVE_INCREASE_TIME = 5f;
   public const float MIX_WAVE_TIME = 120;

   public const float ENEMY_HP_INCREASE_INTERVAL = 10f; // 페이즈 시작 후 이 시간(초)마다 체력 증가
   public const float ENEMY_HP_REACH_RATIO = 0.5f; // 웨이브 끝에 (다음 페이즈 체력 - 현재 페이즈 체력)의 10%까지 도달

   public const int INIT_ITEM_PRICE = 10;
   public const int INCREASE_ITEM_PRICE = 5;
   public const int INIT_SPAWN_PRICE = 10;
   public const int INCREASE_SPAWN_PRICE = 1;



   // public const int MIN_SLIME_SLOT_COUNT = 6;
   // public const int MAX_SLIME_SLOT_COUNT = 9;
   public const int LEVEL_TO_GROWUP2 = 2;//0,1,2 되어야됌



}
