using UnityEngine;


public class EliteEnemy : NormalEnemy
{
    [SerializeField] float patternCoolTime;
    [SerializeField] EnemyAttackPattern enemyAttackPattern;


    protected override void ReachPathEnd()
    {
        rg2d.linearVelocity = Vector2.zero;
        damageData.damage = 10;
        Character.Instance.TakeDamage(damageData);
        Destroy();
    }
    public override void Reward()
    {
        int gold = 100;

        Character.Instance.AddDiceCoin(2);
        DiceCoinText.Instantiate().SetDiceCoinText(transform.position, $"+{2}");    
        Character.Instance.AddCoin(gold);
        CoinText.Instantiate().SetCoinText(transform.position, $"+{gold}");
    }
}
