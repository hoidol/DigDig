using UnityEngine;

public class GameSetting
{
   // public const int MAX_EQUIPT_BULLET_COUNT = 5;
   // public const int MAX_MF_GROUP_POINT = 5; //MemoryFragment
   public static string FIRST_STAGE_KEY = "Normal_0";
   public static CharacterName INIT_CHARACTER_NAME = CharacterName.Lucky;
   public static string[] INIT_SLIME_KEYS = { "Flame", "Ice", "Wacky", "Orbit", "Lightning" };
   public static int SLIME_SLOT_COUNT = 5;
   public static int PLAY_COST_ENERGE = 5;
   public static int MAX_ENERGE = 30;

   public const int BOSS_PHASE = 19; //총 20 Phase
   // public const float BREAK_TIME = 20;

   public const float WAVE_TIME = 30;
   public const float BREAK_TIME = 10;

   // public const float CAN_START_NEXT_PHASE_TIME = 30; //해당 초가 지나면 바로 스킵 가능, 스킵 시 보상
   public const int SKIP_WAVE_GOLD = 5; // 스킵하면 받는 골드 (고정)
   // public const float SKIP_WAVE_END_BEFORE = 5; // 웨이브 종료 n초 전부터 스킵 불가
   // public const float ENEMY_MAX_SPAWN_TIME = 30;
   public const float ENEMY_SPAWN_INTERVAL = 1.25f;

   public const float ENEMY_HP_INCREASE_INTERVAL = 10f; // 페이즈 시작 후 이 시간(초)마다 체력 증가
   public const float ENEMY_HP_REACH_RATIO = 0.5f; // 웨이브 끝에 (다음 페이즈 체력 - 현재 페이즈 체력)의 10%까지 도달

   public const int INIT_ITEM_PRICE = 10;
   public const int INCREASE_ITEM_PRICE = 5;
   public const int INIT_SPAWN_PRICE = 10;
   public const int INCREASE_SPAWN_PRICE = 1;
   public const int BLOCKED_TILE_COUNT = 3; // 게임 시작 시 랜덤으로 막는 타일 수



   // public const int MIN_SLIME_SLOT_COUNT = 6;
   // public const int MAX_SLIME_SLOT_COUNT = 9;
   public const int LEVEL_TO_GROWUP2 = 2;//0,1,2 되어야됌



}
