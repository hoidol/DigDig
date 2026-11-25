using UnityEngine;

public class GameSetting
{
   // public const int MAX_EQUIPT_BULLET_COUNT = 5;
   // public const int MAX_MF_GROUP_POINT = 5; //MemoryFragment
   public static string FIRST_STAGE_KEY = "Normal_0"; // 스테이지 key = "{난이도}_{order}" (StageData.MakeKey, order 0부터)
   public static KingSlimeName INIT_CHARACTER_NAME = KingSlimeName.Lucky;
   // (2026-11-20) 초기 지급·레벨 지급 미니언 = Resources/MinionUnlockSetting 에셋에서 설정 (수확·팔랑이는 게임에서 뺌)
   public static string[] INIT_MINION_KEYS => MinionUnlockSetting.InitKeys;
   public static (string key, int lv)[] MINION_LEVEL_UNLOCKS =>
      System.Array.ConvertAll(MinionUnlockSetting.LevelUnlocks, e => (e.key, e.level));
   // (2026-11-19) wave마다 카드 3장 지급 → 드래그로 소환 (MinionOfferSystem). 골드 소환·강화·코인 표시는 숨김. 합성 결과 = 같은 종
   public const bool USE_WAVE_MINION_OFFER = true;
   // (2026-11-19) 카드 소환 비용 = 주사위 코인(KingSlime.diceCoin). 브레이크타임마다 WAVE_DICE_COIN 지급, 카드 가격 = MinionData.offerDiceCoinPrice(기본값이 OFFER_MINION_DICE_COIN_PRICE)
   public const int WAVE_DICE_COIN = 5;
   public const int OFFER_MINION_DICE_COIN_PRICE = 2;
   // 게임에서 쓰는 미니언 =초기 지급 + 레벨 지급. 뽑기·미니언 관리 화면은 이 목록만
   public static bool IsUsedMinion(string key) =>
      System.Array.IndexOf(INIT_MINION_KEYS, key) >= 0 || System.Array.FindIndex(MINION_LEVEL_UNLOCKS, e => e.key == key) >= 0;
   // 미니언 표시 순서 (등급 없음, 2026-11-20) = 초기 지급 순서 → 레벨 지급 순서. 목록에 없으면 맨 뒤
   public static int GetMinionOrder(string key)
   {
      int idx = System.Array.IndexOf(INIT_MINION_KEYS, key);
      if (idx >= 0)
         return idx;
      idx = System.Array.FindIndex(MINION_LEVEL_UNLOCKS, e => e.key == key);
      return idx >= 0 ? INIT_MINION_KEYS.Length + idx : int.MaxValue;
   }
   public static int MINION_SLOT_COUNT = 5;
   public static int PLAY_COST_ENERGE = 5;
   public static int MAX_ENERGE = 30;

   // 마지막 wave(표시 번호, 2026-11-19 무한 → 80, 2026-11-20 → 60). 이 wave(보스)를 처치하면 게임 클리어
   public const int LAST_WAVE = 60;
   // N번째 wave(1부터)가 BOSS_WAVE_INTERVAL의 배수면 보스, ELITE_WAVE_INTERVAL의 배수면 엘리트 (보스 우선)
   public const int ELITE_WAVE_INTERVAL = 5;
   public const int BOSS_WAVE_INTERVAL = 20;
   // 일반 적 처치 코인이 이 wave 수마다 +1 (NormalEnemy.GetKillGold, 2026-11-19 이전 10)
   public const int KILL_GOLD_WAVE_INTERVAL = 20;

   // (2026-11-22) 인게임 킹 레벨: 경험치는 주사위 코인으로 구매만 (처치 경험치 없음, KingExpBuyPanel)
   // (2026-11-23) 최대 Lv.9, 필요 경험치 = 레벨마다 표 (Lv.0→1 ~ Lv.8→9). 최대 레벨이면 경험치 MAX (더 못 삼)
   // (2026-11-24) 최대 Lv.8 (Lv.8 = 3단계 스킬 "용사가 될거야!"). 표는 Lv.0→1 ~ Lv.7→8
   public const int KING_MAX_LEVEL = 8;
   public static readonly int[] KING_LEVEL_EXPS = { 2, 2, 2, 3, 3, 3, 4, 4 };
   // (2026-11-24) 용사 에너지: wave 중 HERO_ENERGY_CHARGE_TIME초마다 1개 충전
   //   (2026-11-25 삭제: 정예병 칸 미니언 처치로 충전 시간 감소)
   // (2026-11-25) 충전 60 → 30초, 최대 개수 = 킹 레벨 기준 (Lv.2 = 2개, Lv.4 = 3, Lv.6 = 4, Lv.8 = 5. 이전: 해방한 스킬 수)
   public const float HERO_ENERGY_CHARGE_TIME = 30f;
   // (2026-11-25) 에너지 최대 = 1단계 스킬 해방 때 2개, 이후 자동 증가 없음 → 용사 강화 카드 "에너지 보유 증가"(+1/Lv, 최대 3)로만 (이전: 킹 레벨 기준 2→3→4→5)
   public const int HERO_ENERGY_BASE_MAX = 2;
   public const float HERO_MIN_CHARGE_TIME = 15f; // 충전 가속 카드를 다 골라도 이 아래로는 안 줄어듦
   // 경험치 구매: 주사위 코인 BUY_EXP_DICE_COIN_PRICE개 → 경험치 BUY_EXP_AMOUNT
   public const int BUY_EXP_DICE_COIN_PRICE = 2;
   public const int BUY_EXP_AMOUNT = 1;
   // (2026-11-23) 킹 레벨업 보상: 레벨이 오를 때마다 주사위 코인 +1 (KingSlime.AddExp, 레벨업 창 아래에 표시)
   public const int LEVEL_UP_DICE_COIN = 1;
   // 보너스 엘리트: 다음 브레이크타임 주사위 코인 +1 (경험치 없음)
   public const int BONUS_ELITE_NEXT_DICE_COIN = 1;
   // 보스 wave(20 wave) 구간 체력 배율의 기본값: StageData.csv CycleHpMuls가 비었을 때만 구간마다 곱함 (StageData.GetCycleHpMul)
   public const float HP_MUL_PER_BOSS_CYCLE = 1.1f;

   public const float WAVE_TIME = 90; // (2026-11-24) 카드 소환 모드에선 시간 제한 없음(시간 초과 처리 삭제). 카드 소환 모드가 아닐 때의 wave 시간
   public const int WAVE_ENEMY_COUNT = 40; // 한 wave에 소환하는 일반 적 총 수 (ENEMY_SPAWN_INTERVAL 간격, 2026-11-15 시간 기준 → 마리 수 기준)
   public const float ENEMY_SPAWN_INTERVAL = 0.5f;

   public const float ENEMY_HP_INCREASE_INTERVAL = 10f; // 페이즈 시작 후 이 시간(초)마다 체력 증가
   public const float ENEMY_HP_REACH_RATIO = 0.5f; // 웨이브 끝에 (다음 페이즈 체력 - 현재 페이즈 체력)의 10%까지 도달
   // public const int DRAW_ITEM_DICE_COIN_PRICE = 15; // 타일 아이템 뽑기 가격 = 주사위 코인 고정 (뽑을수록 오르지 않음, 2026-11-14)
   // public const int INIT_DRAW_ITEM_CHANCE = 100; // 아이템 뽑기 성공 확률(%). 성공할 때마다 감소
   // public const int DECREASE_DRAW_ITEM_CHANCE = 5;
   // public const int MIN_DRAW_ITEM_CHANCE = 10;
   // public const int SELL_TILE_ITEM_PRICE = 2; // 타일 아이템 판매 시 받는 코인
   // public const int SELL_MINION_BASE_PRICE = 5; // 미니언 판매 가격 = 5 + 5 × mergeLevel (mergeLevel 0부터)
   // public const int SELL_MINION_PRICE_PER_MERGE_LEVEL = 5;
   // public const int INIT_SPAWN_PRICE = 10;
   // public const int INCREASE_SPAWN_PRICE = 1;
   // public const int REINFORCE_PRICE_PER_LEVEL = 50; // 슬라임 종 강화 가격 = 30 × (현재 reinforceLevel + 1) → 30, 60, 90... (최대 레벨 없음)
   public const int BLOCKED_TILE_COUNT = 3; // 게임 시작 시 랜덤으로 막는 타일 수

   public const int LEVEL_TO_GROWUP2 = 2;//0,1,2 되어야됌
   public const int MINION_MIN_EXIST_COUNT = 10;

   public static float ATTACK_SPEED_PER_SECOND = 50f; // AttackSpeed가 이 값이면 초당 1회 공격 (Orbit은 이때 orbitRotationSpeed로 회전)

   // public const int MERGE_REINFORCE_MAX_EXP =3;

   public static float INIT_CRIT_POWER = 1.5f;
}
