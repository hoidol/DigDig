using UnityEngine;

public abstract class Item : PlayerEnhancement
{
    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return itemData.desc;
    }

    public ItemData itemData => ItemManager.Instance.GetItemData(key);

    public bool CanMerge()
    {
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
