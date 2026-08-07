using System;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public string Key=>key;
    public string key;
    // public bool equipped;
    public int count;
    public ReinforceType ReinforceType => ReinforceType.Item;
    public virtual string GetDescription(int lv = 1, bool detail = false)
    {
        if (itemData == null)
            Debug.Log($"GetDescription if(itemData== null) {key}");

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
