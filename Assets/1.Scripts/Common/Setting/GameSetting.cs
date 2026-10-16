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

   public const int BOSS_PHASE = 19; //총 20 Phase
   public const float BREAK_TIME = 20;   

   public const float WAVE_TIME = 60;

   public const float CAN_START_NEXT_PHASE_TIME = 20; //해당 초가 지나면 바로 스킵 가능, 스킵 시 보상
   public const int SKIP_WAVE_MAX_GOLD = 10; // CAN_START_NEXT_PHASE_TIME에 스킵하면 받는 골드, 이후 점점 감소
   public const float SKIP_WAVE_END_BEFORE = 5; // 웨이브 종료 n초 전에 보상 0 + 스킵 불가
   public const float ENEMY_MAX_SPAWN_TIME = 45;
   public const float ENEMY_SPAWN_INTERVAL = 0.75f;

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
