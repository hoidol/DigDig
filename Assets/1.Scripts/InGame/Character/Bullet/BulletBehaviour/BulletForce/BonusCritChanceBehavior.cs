using UnityEngine;

// 크리티컬 확률 추가 보너스 적용 (캐릭터 치명타 확률보다 낮으면 캐릭터 값 유지)
public class BonusCritChanceBehavior : IBulletForce
{
    float bonusChance;

    public BonusCritChanceBehavior(float bonusChance) => this.bonusChance = bonusChance;

    public float GetMultiDamage(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        AllyUnitDamageData d = ((AllyBulletObject)bullet).DamageData;
        d.critChance = Mathf.Max(d.critChance, bonusChance);
        return 0;
    }
}
