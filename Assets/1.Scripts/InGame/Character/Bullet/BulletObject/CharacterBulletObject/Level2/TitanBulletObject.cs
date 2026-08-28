using UnityEngine;

public class TitanBulletObject : CharacterBulletObject
{
    int killCount;
     int lv;
     DamageBoostForce damageBoostForce;

    public override IHittable Hit(RaycastHit2D hit2D)
    {

        IHittable result = base.Hit(hit2D);

        if(result == null)
            return null;

        if(hit2D.transform.TryGetComponent(out IHittable enemy))
        {
            if(enemy.CurHp <=0)
            {
                killCount++;
            }
        }

        float scale = 1f + killCount * TitanBulletSpec.SIZE_PER_KILL[lv - 1];
        transform.localScale = Vector3.one * scale;

        if (killCount > 0)
        {
            if(damageBoostForce != null)
            {
                RemoveBulletForce(damageBoostForce);
            }
            damageBoostForce = new DamageBoostForce(killCount * TitanBulletSpec.DAMAGE_PER_KILL[lv - 1]);
            AddBulletForce(damageBoostForce);
        }
            

        return result;
    }
}