using UnityEngine;


public class EliteEnemy : NormalEnemy
{
    [SerializeField] float patternCoolTime;
    [SerializeField] EnemyAttackPattern enemyAttackPattern;


    protected override void ReachPathEnd()
    {
        rg2d.linearVelocity = Vector2.zero;
        damageData.damage = 10;
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
        if (enemyType == EnemyType.BonusElite)
        {
            KingSlime.Instance.AddDiceCoin(BONUS_ELITE_DICE_COIN);
            DiceCoinText.Instantiate().SetDiceCoinText(transform.position, $"+{BONUS_ELITE_DICE_COIN}");
            return;
        }

        KingSlime.Instance.AddDiceCoin(ELITE_DICE_COIN);
        DiceCoinText.Instantiate().SetDiceCoinText(transform.position, $"+{ELITE_DICE_COIN}");

        int gold = GetEliteRewardGold(wave);
        KingSlime.Instance.AddCoin(gold);
        CoinText.Instantiate().SetCoinText(transform.position, $"+{gold}");
    }
}
