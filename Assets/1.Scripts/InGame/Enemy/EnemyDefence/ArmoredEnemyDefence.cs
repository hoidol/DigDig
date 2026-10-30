using UnityEngine;

// 갑옷형 - 물리 내성, 마법 약점
public class ArmoredEnemyDefence : EnemyDefence
{
    public override DefenceType DefenceType => DefenceType.Armored;

    public override float CalculateDamage(DamageData damage)
    {
        return GetMultiplier(damage, DefenceType.Armored);
    }
}
