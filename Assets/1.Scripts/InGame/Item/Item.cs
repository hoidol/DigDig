using UnityEngine;
public abstract class Item : MonoBehaviour
{
    public string key;
    public int count; //해당아이템을 중복으로 가지고 있는 개수
    public virtual string GetDescription(int c = -1)
    {
        if (c <= 0)
            c = count;
        return "아이템을 설명합니다";
    }
    public ItemData GetItemData()
    {
        return ItemManager.Instance.GetItemData(key);
    }
    public virtual void OnEquip(Player player)
    {
        UpdateItem();
    }
    public abstract void OnUnequip(Player player);
    public virtual void UpdateItem()
    {

    }
}