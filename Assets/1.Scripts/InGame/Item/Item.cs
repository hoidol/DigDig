using System;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public string Key=>key;
    public string key;
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
        
    }


}
