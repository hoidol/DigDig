using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BulletInventory : MonoBehaviour
{
    public List<string> curBullets = new List<string>();
    //public List<BulletStat
    // public List<MergeBulletData> canMergeBulletDatas = new List<MergeBulletData>();
    //public readonly int MAX_ITEM_COUNT = 8;

    // 인터페이스별 캐시 - 장착/해제 시점에만 갱신
    public List<IPreFire> preFires = new List<IPreFire>();
    public List<IFired> fireds = new List<IFired>();
    // public List<IComboAttack> comboAttacks = new List<IComboAttack>();
    public List<IBullet> bullets = new List<IBullet>();
    void RefreshCache()
    {
        preFires = curBullets.OfType<IPreFire>().ToList();
        fireds = curBullets.OfType<IFired>().ToList();
        // comboAttacks = curBullets.OfType<IComboAttack>().ToList();
        bullets = curBullets.OfType<IBullet>().ToList();
    }

    void Awake()
    {
    }

    public bool CheckHave(string key)
    {
        return curBullets.Any(b => b == key);
    }


    public void ReinforceBullet(string key, int count)
    {

    }

    public int GetMaxBulletCount()
    {
        return curBullets.Count;
    }
    public void AddBullet(string key)
    {
        AddBullet(BulletData.GetBulletData(key));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bulletData"></param>
    /// <param name="openChangeBullet"> 아이템 더이상 획득 못하면 교체창 열기</param>
    public void AddBullet(BulletData bulletData)
    {
        //Bullet bullet = BulletManager.Create(bulletData.key);
        curBullets.Insert(0, bulletData.key);//맨 앞으로
        Character.Instance.UpdateCharacter();
        RefreshCache();
    }

    public void ReleaseBullet(string key)
    {
        curBullets.Remove(key);
        RefreshCache(); // RefreshCache 포함
    }


    public List<MergeBulletData> GetCanMergeBulletData()
    {
        List<MergeBulletData> canMergeBulletDatas = new List<MergeBulletData>();
        for (int i = 0; i < BulletManager.Instance.mergeBulletDatas.Length; i++)
        {
            // MergeBulletData mergeBulletData = BulletManager.Instance.mergeBulletDatas[i];
            // if (mergeBulletData.resourceBulletKeys.All(k => Character.Instance.weapon.bulletInventory.curBullets.Any(b => b == k)))
            //     canMergeBulletDatas.Add(mergeBulletData);
        }
        return canMergeBulletDatas;
    }


}

public class UpdaterMergeRecommendBulletEvent
{
    public List<MergeBulletData> recommendMergeBullets;
    public UpdaterMergeRecommendBulletEvent(List<MergeBulletData> list)
    {
        recommendMergeBullets = list;
    }

}



public class AddedBulletEvent
{
    public BulletData bulletData;
    public AddedBulletEvent(BulletData bulletData)
    {
        this.bulletData = bulletData;
    }
}

public class RemovedBulletEvent
{
    public BulletData bulletData;
    public RemovedBulletEvent(BulletData bulletData)
    {
        this.bulletData = bulletData;
    }
}

