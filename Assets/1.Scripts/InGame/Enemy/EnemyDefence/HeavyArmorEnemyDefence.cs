using UnityEngine;

// 중갑형 - 물리/마법 내성, 진동 약점
public class HeavyArmorEnemyDefence : EnemyDefence
{
    public override DefenceType DefenceType => DefenceType.HeavyArmor;

    public override float CalculateDamage(DamageData damage)
    {
        return GetMultiplier(damage, DefenceType.HeavyArmor);
    }
}
