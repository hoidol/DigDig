
using UnityEngine;
public class OreDamageBehavior : IBulletForce
{
    public float bonusDamageRate;

    public OreDamageBehavior(float rate)
    {
        this.bonusDamageRate = rate;
    }

    public float GetMultiDamage(BulletBase bullet, IHittable hit, RaycastHit2D hit2D)
    {
        if (hit.Transform.CompareTag("OreStone"))
        {
            return bullet.damage * bonusDamageRate;
        }
        else
        {
            return 0;
        }
    }


}
