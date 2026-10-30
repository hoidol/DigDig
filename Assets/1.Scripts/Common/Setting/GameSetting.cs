using UnityEngine;

public class GameSetting
{
   // public const int MAX_EQUIPT_BULLET_COUNT = 5;
   // public const int MAX_MF_GROUP_POINT = 5; //MemoryFragment
   public static string FIRST_STAGE_KEY = "Normal"; // 난이도마다 스테이지 하나, key = 난이도 이름
   public static CharacterName INIT_CHARACTER_NAME = CharacterName.Lucky;
   public static string[] INIT_SLIME_KEYS = { "Flame", "Ice", "Wacky", "Orbit", "Lightning" };
   public static int SLIME_SLOT_COUNT = 5;
   public static int PLAY_COST_ENERGE = 5;
   public static int MAX_ENERGE = 30;

   // 무한 웨이브: N번째 wave(1부터)가 BOSS_WAVE_INTERVAL의 배수면 보스, ELITE_WAVE_INTERVAL의 배수면 엘리트 (보스 우선)
   public const int ELITE_WAVE_INTERVAL = 5;
   public const int BOSS_WAVE_INTERVAL = 20;

   public const float WAVE_TIME = 60;
   public const float WAVE_SPAWN_TIME = 40;
   public const float SKIP_SHOW_DELAY = 5; // 웨이브 시작 후 스킵 버튼이 보이기까지 시간
   public const float ENEMY_SPAWN_INTERVAL = 1f;

   public const float ENEMY_HP_INCREASE_INTERVAL = 10f; // 페이즈 시작 후 이 시간(초)마다 체력 증가
   public const float ENEMY_HP_REACH_RATIO = 0.5f; // 웨이브 끝에 (다음 페이즈 체력 - 현재 페이즈 체력)의 10%까지 도달

   public const int INIT_ITEM_PRICE = 10;
   public const int INCREASE_ITEM_PRICE = 5;
   public const int INIT_DRAW_ITEM_PRICE = 10;
   public const int INCREASE_DRAW_ITEM_PRICE = 2;
   public const int INIT_DRAW_ITEM_CHANCE = 100; // 아이템 뽑기 성공 확률(%). 성공할 때마다 감소
   public const int DECREASE_DRAW_ITEM_CHANCE = 5;
   public const int MIN_DRAW_ITEM_CHANCE = 10;
   public const int SELL_TILE_ITEM_PRICE = 10; // 타일 아이템 판매 시 받는 코인
   public const int INIT_SPAWN_PRICE = 10;
   public const int INCREASE_SPAWN_PRICE = 1;
   public const int BLOCKED_TILE_COUNT = 3; // 게임 시작 시 랜덤으로 막는 타일 수

   public const int LEVEL_TO_GROWUP2 = 2;//0,1,2 되어야됌
   public const int SLIME_MIN_EXIST_COUNT = 10;

   public static float ATTACK_SPEED_PER_SECOND = 50f; // AttackSpeed가 이 값이면 초당 1회 공격 (Orbit은 이때 orbitRotationSpeed로 회전)

   // public const int MERGE_REINFORCE_MAX_EXP =3;


}
