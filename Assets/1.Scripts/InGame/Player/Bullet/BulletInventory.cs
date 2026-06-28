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
    public List<IPreAttack> preAttacks = new List<IPreAttack>();
    public List<IAttack> attacks = new List<IAttack>();
    public List<IComboAttack> comboAttacks = new List<IComboAttack>();
    public List<IBullet> bullets = new List<IBullet>();
    void RefreshCache()
    {
        preAttacks = curBullets.OfType<IPreAttack>().ToList();
        attacks = curBullets.OfType<IAttack>().ToList();
        comboAttacks = curBullets.OfType<IComboAttack>().ToList();
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

    // List<string> bulletSkillKeys = new List<string>();
    // public List<string> GetBulletKeys()
    // {
    //     bulletSkillKeys.Clear();
    //     for (int i = 0; i < Player.Instance.weapon.bulletInventory.curBullets.Count; i++)
    //     {
    //         bulletSkillKeys.Add(Player.Instance.weapon.bulletInventory.curBullets[i]);
    //     }
    //     return bulletSkillKeys;
    // }
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
        curBullets.Insert(0, bulletData.key);
        Player.Instance.UpdatePlayer();
        RefreshCache();
    }

    public void ReleaseBullet(string key)
    {

        curBullets.Remove(key);

        RefreshCache(); // RefreshCache 포함
    }



    // public Bullet GetBullet(int bulletIdx)
    // {
    //     return curBullets[bulletIdx];
    // }

    public void CheckMerge()
    {
        // canMergeBulletDatas.Clear();
        MergeBulletData[] mergeBulletDatas = BulletManager.Instance.mergeBulletDatas;

        // var mergeableBulletKeys = new HashSet<string>(
        //     curBullets.Where(bKey => BulletManager.bullets[bKey].CanMerge()).Select(bulletSkill => bulletSkill.key)
        // );

        // var ownedSkillKeys = new HashSet<string>(
        //     Player.Instance.abilityInventory.equippedAbilitys.Select(s => s.key)
        // );

        // foreach (var m in mergeBulletDatas)
        // {
        //     bool bulletSkillsOk = m.resourceBulletKeys.All(k => mergeableBulletKeys.Contains(k));
        //     bool abilityOk = m.resourceAbilityKeys == null || m.resourceAbilityKeys.All(k => ownedSkillKeys.Contains(k));
        //     if (bulletSkillsOk && abilityOk)
        //         canMergeBulletDatas.Add(m);
        // }

        // GameEventBus.Publish(new UpdaterMergeRecommendBulletEvent(canMergeBulletDatas));
    }

    public List<MergeBulletData> GetCanMergeBulletData()
    {
        List<MergeBulletData> canMergeBulletDatas = new List<MergeBulletData>();
        for (int i = 0; i < BulletManager.Instance.mergeBulletDatas.Length; i++)
        {
            MergeBulletData mergeBulletData = BulletManager.Instance.mergeBulletDatas[i];
            if (mergeBulletData.resourceBulletKeys.All(k => Player.Instance.weapon.bulletInventory.curBullets.Any(b => b == k)))
                canMergeBulletDatas.Add(mergeBulletData);
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

