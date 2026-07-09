using UnityEngine;

public class TitanBulletObject : PlayerBulletObject
{
    int killCount;
     int lv;
     DamageBoostForce damageBoostForce;
     public override void Shoot(Vector2 dir, float damage)
    {
        base.Shoot(dir, damage);

        lv = Player.Instance.statMgr.bulletStatDic[key].lv;
    }

    public override IHittable Hit(RaycastHit2D hit2D)
    {

        IHittable result = base.Hit(hit2D);

        if(result == null)
            return null;

        if(hit2D.transform.TryGetComponent(out Enemy enemy))
        {
            if(enemy.state== EnemyState.Dead)
            {
                killCount++;
            }
        }
        else if(hit2D.transform.TryGetComponent(out OreStone oreStone))
        {
            if(oreStone.curHp <=0)
            {
                killCount++;
            }
        }

        float scale = 1f + killCount * TitanBullet.SIZE_PER_KILL[lv - 1];
        transform.localScale = Vector3.one * scale;

        if (killCount > 0)
        {
            if(damageBoostForce != null)
            {
                RemoveBulletForce(damageBoostForce);
            }
            damageBoostForce = new DamageBoostForce(killCount * TitanBullet.DAMAGE_PER_KILL[lv - 1]);
            AddBulletForce(damageBoostForce);
        }
            

        return result;
    }
}