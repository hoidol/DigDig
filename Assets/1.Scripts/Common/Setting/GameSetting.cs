using UnityEngine;

public class GameSetting
{
   // public const int MAX_EQUIPT_BULLET_COUNT = 5;
   // public const int MAX_MF_GROUP_POINT = 5; //MemoryFragment
   public static string FIRST_STAGE_KEY = "Normal"; // 난이도마다 스테이지 하나, key = 난이도 이름
   public static KingSlimeName INIT_CHARACTER_NAME = KingSlimeName.Lucky;
   public static string[] INIT_MINION_KEYS = { "Flame", "Ice", "Random", "Orbit", "Lightning" };
   public static int MINION_SLOT_COUNT = 5;
   public static int PLAY_COST_ENERGE = 5;
   public static int MAX_ENERGE = 30;

   // 마지막 wave(표시 번호, 2026-11-19 무한 → 80). 이 wave(보스)를 처치하면 게임 클리어
   public const int LAST_WAVE = 80;
   // N번째 wave(1부터)가 BOSS_WAVE_INTERVAL의 배수면 보스, ELITE_WAVE_INTERVAL의 배수면 엘리트 (보스 우선)
   public const int ELITE_WAVE_INTERVAL = 5;
   public const int BOSS_WAVE_INTERVAL = 20;
   // 일반 적 처치 코인이 이 wave 수마다 +1 (NormalEnemy.GetKillGold, 2026-11-19 이전 10)
   public const int KILL_GOLD_WAVE_INTERVAL = 20;
   // 보스 wave(20 wave) 구간 체력 배율의 기본값: StageData.csv CycleHpMuls가 비었을 때만 구간마다 곱함 (StageData.GetCycleHpMul)
   public const float HP_MUL_PER_BOSS_CYCLE = 1.1f;
   // N번째 wave가 DEFENCE_WAVE_INTERVAL의 배수면 일반 적이 Ethereal → Armored 순서로 번갈아 등장, 나머지는 Normal
   public const int DEFENCE_WAVE_INTERVAL = 3;

   public const float WAVE_TIME = 35;
   public const int WAVE_ENEMY_COUNT = 40; // 한 wave에 소환하는 일반 적 총 수 (ENEMY_SPAWN_INTERVAL 간격, 2026-11-15 시간 기준 → 마리 수 기준)
   public const float ENEMY_SPAWN_INTERVAL = 0.5f;

   public const float ENEMY_HP_INCREASE_INTERVAL = 10f; // 페이즈 시작 후 이 시간(초)마다 체력 증가
   public const float ENEMY_HP_REACH_RATIO = 0.5f; // 웨이브 끝에 (다음 페이즈 체력 - 현재 페이즈 체력)의 10%까지 도달
   public const int DRAW_ITEM_DICE_COIN_PRICE = 15; // 타일 아이템 뽑기 가격 = 주사위 코인 고정 (뽑을수록 오르지 않음, 2026-11-14)
   public const int INIT_DRAW_ITEM_CHANCE = 100; // 아이템 뽑기 성공 확률(%). 성공할 때마다 감소
   public const int DECREASE_DRAW_ITEM_CHANCE = 5;
   public const int MIN_DRAW_ITEM_CHANCE = 10;
   public const int SELL_TILE_ITEM_PRICE = 2; // 타일 아이템 판매 시 받는 코인
   public const int SELL_MINION_BASE_PRICE = 5; // 미니언 판매 가격 = 5 + 5 × mergeLevel (mergeLevel 0부터)
   public const int SELL_MINION_PRICE_PER_MERGE_LEVEL = 5;
   public const int INIT_SPAWN_PRICE = 10;
   public const int INCREASE_SPAWN_PRICE = 1;
   public const int REINFORCE_PRICE_PER_LEVEL = 50; // 슬라임 종 강화 가격 = 30 × (현재 reinforceLevel + 1) → 30, 60, 90... (최대 레벨 없음)
   public const int BLOCKED_TILE_COUNT = 3; // 게임 시작 시 랜덤으로 막는 타일 수

   public const int LEVEL_TO_GROWUP2 = 2;//0,1,2 되어야됌
   public const int MINION_MIN_EXIST_COUNT = 10;

   public static float ATTACK_SPEED_PER_SECOND = 50f; // AttackSpeed가 이 값이면 초당 1회 공격 (Orbit은 이때 orbitRotationSpeed로 회전)

   // public const int MERGE_REINFORCE_MAX_EXP =3;

   public static float INIT_CRIT_POWER = 1.5f;
}
