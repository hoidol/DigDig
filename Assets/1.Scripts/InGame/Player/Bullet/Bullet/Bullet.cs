using System.Linq;
using UnityEngine;
[System.Serializable]
public abstract class Bullet : IBullet, IReinforce
{
    public string key;
    public BulletData bulletData => BulletManager.Instance.GetBulletData(key);

    public virtual void OnBulletFired(PlayerBulletObject bullet)
    {
        bullet.AddBehavior(new BounceBehavior(Player.Instance.bounce));
    }

    public virtual bool CanMerge()
    {
        Player.Instance.weapon.bulletInventory.curBullets.Any(x =>
        {
            if (!x.bulletData.specialBullet)
                return false;
            if (x == this)
                return false;


            return bulletData.mergeKeys.Contains(x.key);
        });

        return false;
    }

    public virtual PlayerBulletObject GetBulletObject()
    {
        return BulletManager.Instance.GetPlayerBulletObject(key);
    }
    public virtual string GetDescription(bool detail = false)
    {
        return $"탄 설명";
    }

    public int GetLevel()
    {
        return Player.Instance.statMgr.bulletStatDic[key].lv;
    }
}