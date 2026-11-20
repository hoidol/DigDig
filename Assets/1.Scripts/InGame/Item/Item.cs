using System;
using UnityEngine;

// 아이템 = 능력(Ability)의 한 종류. 데이터는 ItemData(itemPrefab이 이 컴포넌트를 참조)
public abstract class Item : Ability
{
    public override string GetDescription()
    {
        return itemData.desc;
    }
    public ItemData itemData => ItemManager.Instance.GetItemData(key);

    public bool CanMerge()
    {
        return true;
    }

    public override void OnEquip()
    {
        UpdateItem();
    }

    public virtual void UpdateItem()
    {

    }


}
