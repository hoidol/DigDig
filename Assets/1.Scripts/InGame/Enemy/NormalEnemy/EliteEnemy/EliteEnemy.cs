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

    // 엘리트 경험치 = 10 + 5 × (엘리트 순번 - 1): wave 5 = 10, 10 = 15, 15 = 20 …
    public static int GetEliteExp(int waveIdx)
    {
        int step = Mathf.Max(1, (waveIdx + 1) / GameSetting.ELITE_WAVE_INTERVAL);
        return GameSetting.ELITE_EXP_BASE + GameSetting.ELITE_EXP_PER_STEP * (step - 1);
    }

    public override void Reward()
    {
        // 보너스 엘리트 = 주사위 5개만
        // 카드 소환 모드: 주사위 코인은 wave 준비 단계 지급만 (처치로는 못 얻음)
        bool giveDiceCoin = !GameSetting.USE_WAVE_MINION_OFFER;
        if (enemyType == EnemyType.BonusElite)
        {
            // (2026-11-20) 경험치 10 고정 + 다음 브레이크타임 주사위 코인 +1
            KingSlime.Instance.AddExp(GameSetting.BONUS_ELITE_EXP);
            if (giveDiceCoin)
            {
                KingSlime.Instance.AddDiceCoin(BONUS_ELITE_DICE_COIN);
                DiceCoinText.Instantiate().SetDiceCoinText(transform.position, $"+{BONUS_ELITE_DICE_COIN}");
            }
            else
            {
                KingSlime.Instance.bonusDiceCoinNextBreak += GameSetting.BONUS_ELITE_NEXT_DICE_COIN;
                DiceCoinText.Instantiate().SetDiceCoinText(transform.position,
                    string.Format(TranslateManager.GetText("BonusElite_NextDiceCoin"), GameSetting.BONUS_ELITE_NEXT_DICE_COIN));
            }
            return;
        }

        KingSlime.Instance.AddExp(GetEliteExp(wave));

        if (giveDiceCoin)
        {
            KingSlime.Instance.AddDiceCoin(ELITE_DICE_COIN);
            DiceCoinText.Instantiate().SetDiceCoinText(transform.position, $"+{ELITE_DICE_COIN}");
        }

        int gold = GetEliteRewardGold(wave);
        KingSlime.Instance.AddCoin(gold);
        SoundManager.Play(SFXType.CoinGet);
        CoinText.Instantiate().SetCoinText(transform.position, $"+{gold}");
    }
}
