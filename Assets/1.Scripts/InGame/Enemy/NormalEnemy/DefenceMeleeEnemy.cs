using UnityEngine;
public class DefenceMeleeEnemy : MeleeEnemy
{
    public override void TakeDamage(DamageData damage)
    {
        if (state == NormalEnemyState.Dead)
            return;

        if (damage is CharacterBulletDamageData pBDamage)
        {
            if (!InGameUtil.CheckBackAttack(transform, face, pBDamage.hit2D.point))
                return;
        }
        base.TakeDamage(damage);
    }
}