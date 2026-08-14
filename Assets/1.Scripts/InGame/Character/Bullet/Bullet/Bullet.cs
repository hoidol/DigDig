using System.Linq;
using UnityEngine;
[System.Serializable]
public abstract class Bullet 
{
    public string Key => key;
    public string key;
    public BulletData bulletData => BulletManager.Instance.GetBulletData(key);
    public bool mustCrit;
    public virtual CharacterBulletObject Instantiate()
    {
        CharacterBulletObject playerBulletObject = BulletManager.Instance.GetPlayerBulletObject(key);
        playerBulletObject.SetBullet(this);
        return playerBulletObject;
    }


}