using UnityEngine;

public abstract class Item : PlayerEnhancement
{
    public int count;
    public override string GetDescription(bool detail = false)
    {
        Debug.Log($"gameObject.name {gameObject.name}");
        return itemData.desc;
    }

    public ItemData itemData => ItemManager.Instance.GetItemData(key);

    public bool CanMerge()
    {
        return true;
    }

    public override void OnEquip(Player player)
    {
        base.OnEquip(player);
        UpdateItem();
    }

    public abstract override void OnUnequip(Player player);

    public virtual void UpdateItem() { }

    public override void UpdateEnhancement() => UpdateItem();
}
