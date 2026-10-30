using UnityEngine;

// 일반형 - 약점/내성 없음
public class NormalEnemyDefence : EnemyDefence
{
    public override DefenceType DefenceType => DefenceType.Normal;

    public override float CalculateDamage(DamageData damage)
    {
        return GetMultiplier(damage, DefenceType.Normal);
    }
}
