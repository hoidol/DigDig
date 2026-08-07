using UnityEngine;

// 크리티컬 확률 추가 보너스 적용
public class BonusCritChanceBehavior : IBulletForce
{
    float bonusChance;

    public BonusCritChanceBehavior(float bonusChance) => this.bonusChance = bonusChance;

    public float GetMultiDamage(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        CharacterBulletDamageData d = ((CharacterBulletObject)bullet).damageData;
        if (bonusChance < Character.Instance.statMgr.CritChance)
        {
            bonusChance = Character.Instance.statMgr.CritChance;
        }

        if (Random.Range(0f, 100f) <= bonusChance)
            d.mustCrit = true;

        return 0;
    }
}
