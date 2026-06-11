using System;
using UnityEngine;

public abstract class Item : MonoBehaviour, IReinforce
{
    public string key;
    public bool equipped;

    public virtual string GetDescription(bool detail = false)
    {
        return itemData.desc;
    }
    public ItemData itemData => ItemManager.Instance.GetItemData(key);

    public bool CanMerge()
    {
        return true;
    }

    public virtual void OnEquip(Player player)
    {
        equipped = true;
        UpdateItem();
    }

    public virtual void OnUnequip(Player player)
    {
         equipped = false;
    }

    public virtual void UpdateItem() { }

    public virtual void UpdateEnhancement() => UpdateItem();

    public int GetLevel()
    {
        return Player.Instance.statMgr.itemStatDic[key].lv;
    }
}
