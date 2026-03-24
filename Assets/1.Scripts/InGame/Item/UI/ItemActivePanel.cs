using UnityEngine;
using UnityEngine.UI;

public class ItemActivePanel : ItemDisplayPanel
{
    public Image cooltimeImage;

    ActiveItem activeItem;
    public override void SetItemData(ItemData itemData, int count)
    {
        base.SetItemData(itemData, count);
        Item item = Player.Instance.inventory.GetItem(itemData.key);
        if (item is ActiveItem aItem)
        {
            activeItem = aItem;
            cooltimeImage.gameObject.SetActive(true);
        }
        else
        {
            cooltimeImage.gameObject.SetActive(false);
        }
    }
    public override void UpdateOverlapCount(ItemData itemData, int count)
    {

    }
    void Update()
    {
        if (activeItem == null)
            return;

        if (!activeItem.active)
        {
            cooltimeImage.fillAmount = 0;
            return;
        }

        cooltimeImage.fillAmount = activeItem.CoolTimer / activeItem.coolTime;
    }
}
