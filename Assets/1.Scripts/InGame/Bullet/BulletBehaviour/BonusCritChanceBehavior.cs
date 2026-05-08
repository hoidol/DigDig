using UnityEngine;

// 크리티컬 확률 추가 보너스 적용
public class BonusCritChanceBehavior : IBulletForce
{
    float bonusChance;

    public BonusCritChanceBehavior(float bonusChance) => this.bonusChance = bonusChance;

    public float GetMultiDamage(BulletBase bullet, IHittable hit, RaycastHit2D hit2D)
    {
        PlayerDamageData d = ((PlayerBullet)bullet).damageData;
        if (bonusChance < Player.Instance.statMgr.CritChance)
        {
            bonusChance = Player.Instance.statMgr.CritChance;
        }

        if (Random.Range(0f, 100f) <= bonusChance)
            d.mustCrit = true;

        return 0;
    }
}
