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
    public override void Reward()
    {
        KingSlime.Instance.AddDiceCoin(2);
        DiceCoinText.Instantiate().SetDiceCoinText(transform.position, $"+{2}");

        int gold = 100;
        KingSlime.Instance.AddCoin(gold);
        CoinText.Instantiate().SetCoinText(transform.position, $"+{gold}");
    }
}
