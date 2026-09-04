using System;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public string Key=>key;
    public string key;
    public int count;
    public ReinforceType ReinforceType => ReinforceType.Item;
    public virtual string GetDescription()
    {
        return itemData.desc;
    }
    public ItemData itemData => ItemManager.Instance.GetItemData(key);

    public bool CanMerge()
    {
        return true;
    }
    
    public virtual void OnEquip()
    {
        UpdateItem();
    }

    public virtual void OnUnequip()
    {
    }

    public virtual void UpdateItem()
    {
        count = Character.Instance.statMgr.itemStatDic[key].count;
        // count = Character.Instance.itemInventory.GetItemCount(key);//.itemStatDic[key].count;
    }


    public int GetCount()
    {
        return Character.Instance.statMgr.GetCharacterItemStat(key).count;
    }

    public bool IsMaxLevel()
    {
        return ItemData.MAX_COUNT == count;
    }
}
