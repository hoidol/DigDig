using System.Linq;
using UnityEngine;
[System.Serializable]
public abstract class BulletSpec
{
    public string Key => key;
    public string key;
    public bool mustCrit;
    public virtual AllyBulletObject Instantiate(IAllyUnit allyUnit)
    {
        AllyBulletObject bulletObject = BulletSpawner.Instance.GetBulletObject(key);
        bulletObject.SetBullet(this, allyUnit);
        return bulletObject;
    }


}