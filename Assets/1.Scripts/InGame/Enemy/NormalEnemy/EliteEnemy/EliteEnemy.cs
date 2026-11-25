using UnityEngine;


public class EliteEnemy : NormalEnemy
{
    [SerializeField] float patternCoolTime;
    [SerializeField] EnemyAttackPattern enemyAttackPattern;


    public const int REACH_END_DAMAGE = 7; // (2026-11-22) 엘리트(보너스 엘리트 포함)가 경로 끝에 도착하면 킹 피해 (이전 10)

    protected override void ReachPathEnd()
    {
        rg2d.linearVelocity = Vector2.zero;
        damageData.damage = REACH_END_DAMAGE;
        KingSlime.Instance.TakeDamage(damageData);
        Destroy();
    }
    public const int ELITE_DICE_COIN = 2;
    public const int BONUS_ELITE_DICE_COIN = 5;
    public const int ELITE_GOLD_PER_STEP = 30;

    // 엘리트 처치 골드 = (wave 번호 / 엘리트 주기) × 30 → wave 5 = 30, 10 = 60, 15 = 90 …
    public static int GetEliteRewardGold(int waveIdx)
    {
        int step = Mathf.Max(1, (waveIdx + 1) / GameSetting.ELITE_WAVE_INTERVAL);
        return step * ELITE_GOLD_PER_STEP;
    }

    public override void Reward()
    {
        // 보너스 엘리트 = 주사위 5개만
        // 카드 소환 모드: 주사위 코인은 wave 준비 단계 지급만 (처치로는 못 얻음)
        bool giveDiceCoin = !GameSetting.USE_WAVE_MINION_OFFER;
        if (enemyType == EnemyType.BonusElite)
        {
            // (2026-11-20) 다음 브레이크타임 주사위 코인 +1 (2026-11-22 처치 경험치 삭제)
            if (giveDiceCoin)
            {
                KingSlime.Instance.AddDiceCoin(BONUS_ELITE_DICE_COIN);
                DiceCoinText.Instantiate().SetDiceCoinText(transform.position, $"+{BONUS_ELITE_DICE_COIN}");
            }
            else
            {
                bool now = KingSlime.Instance.AddRewardDiceCoin(GameSetting.BONUS_ELITE_NEXT_DICE_COIN);
                DiceCoinText.Instantiate().SetDiceCoinText(transform.position, now ? $"+{GameSetting.BONUS_ELITE_NEXT_DICE_COIN}" :
                    string.Format(TranslateManager.GetText("BonusElite_NextDiceCoin"), GameSetting.BONUS_ELITE_NEXT_DICE_COIN));
            }
            return;
        }

        if (giveDiceCoin)
        {
            KingSlime.Instance.AddDiceCoin(ELITE_DICE_COIN);
            DiceCoinText.Instantiate().SetDiceCoinText(transform.position, $"+{ELITE_DICE_COIN}");
        }

        // (2026-11-23) 엘리트 처치 골드 없음 (골드 개념 삭제). 이전: GetEliteRewardGold(wave)만큼 코인
        // (2026-11-23) 엘리트 처치 = 다음 브레이크타임 룬 선택 (보너스 엘리트는 위에서 return → 대상 아님)
        RuneRewardSystem.AddReward(transform.position);
    }
}
