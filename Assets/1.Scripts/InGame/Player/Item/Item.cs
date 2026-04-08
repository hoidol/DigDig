using UnityEngine;

public abstract class Item : PlayerEnhancement
{
    public override string GetDescription(int c = -1)
    {
        if (c <= 0) c = count;
        return "아이템을 설명합니다";
    }

    public ItemData itemData => ItemManager.Instance.GetItemData(key);

    public bool CanMerge()
    {
        if (count < ItemData.MAX_OWN_COUNT || itemData.isUnique)
            return false;
        return true;
    }

    public override void OnEquip(Player player)
    {
        UpdateItem();
    }

    public abstract override void OnUnequip(Player player);

    public virtual void UpdateItem() { }

    public override void UpdateEnhancement() => UpdateItem();
}
