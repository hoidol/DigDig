using UnityEngine;

// 생체형 - 물리 약점, 마법/진동 내성
public class OrganicEnemyDefence : EnemyDefence
{
    public override DefenceType DefenceType => DefenceType.Organic;

    public override float CalculateDamage(DamageData damage)
    {
        return GetMultiplier(damage, DefenceType.Organic);
    }
}
