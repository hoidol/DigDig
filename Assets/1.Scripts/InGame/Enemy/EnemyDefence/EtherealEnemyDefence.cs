using UnityEngine;

// 정령형 - 마법 내성, 물리 약점
public class EtherealEnemyDefence : EnemyDefence
{
    public override DefenceType DefenceType => DefenceType.Ethereal;

    public override float CalculateDamage(DamageData damage)
    {
        return GetMultiplier(damage, DefenceType.Ethereal);
    }
}
