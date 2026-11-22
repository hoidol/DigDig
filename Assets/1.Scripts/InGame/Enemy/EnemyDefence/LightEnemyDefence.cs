using UnityEngine;

// 경갑 (2026-11-21). 배율은 DefenceInfo 표
public class LightEnemyDefence : EnemyDefence
{
    public override DefenceType DefenceType => DefenceType.Light;

    public override float CalculateDamage(DamageData damage)
    {
        return GetMultiplier(damage, DefenceType.Light);
    }
}
