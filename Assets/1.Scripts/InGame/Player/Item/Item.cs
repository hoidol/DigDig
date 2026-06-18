using System;
using UnityEngine;

public abstract class Item : MonoBehaviour, IReinforce
{
    public string key;
    public bool equipped;
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


    public int GetLevel()
    {
        return Player.Instance.statMgr.itemStatDic[key].lv;
    }
}
