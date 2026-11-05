using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[System.Serializable]
public abstract class BulletSpec
{
    public string Key => key;
    public string key;
    public bool mustCrit;
    public List<IBulletBehavior> bulletBehaviors = new List<IBulletBehavior>();
    public List<IBulletForce> bulletForces = new List<IBulletForce>();

    public virtual void StartBulletSpec()
    {
        bulletBehaviors.Clear();
        bulletForces.Clear();
    }

    //현재 스펙이 적용된 총알 생성
    public virtual AllyBulletObject Instantiate(IAllyUnit allyUnit)
    {
        AllyBulletObject bulletObject = BulletSpawner.Instance.GetBulletObject(key);
        bulletObject.SetBullet(this, allyUnit);
        return bulletObject;
    }
    public virtual void AddBulletBehaviour(IBulletBehavior bulletBehavior)
    {
        bulletBehaviors.Add(bulletBehavior);
    }
    public virtual void AddBulletForce(IBulletForce bulletForce)
    {
        bulletForces.Add(bulletForce);
    }


}