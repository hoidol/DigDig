using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ItemPickupPanel : MonoBehaviour
{
    public TMP_Text titleText;
    public GameObject singleThumObject;
    public ItemDisplayPanel singleItemDisplayPanel;

    public GameObject upgradeThumObject;

    public ItemDisplayPanel lowerItemDisplayPanel;

    public ItemDisplayPanel higherItemDisplayPanel;
    //public Image thumImage;
    public TMP_Text descText;

    protected ItemData itemData;
    public virtual void SetItemData(ItemData itemData)
    {
        this.itemData = itemData;

        Item item = Player.Instance.inventory.GetItem(itemData.key);
        if (item == null)
        {
            singleThumObject.SetActive(true);
            singleItemDisplayPanel.SetItemData(itemData, item.count);

            upgradeThumObject.SetActive(false);
        }
        else
        {
            singleThumObject.SetActive(false);
            upgradeThumObject.SetActive(true);
            lowerItemDisplayPanel.SetItemData(itemData, item.count);
            higherItemDisplayPanel.SetItemData(itemData, item.count + 1);
        }
        descText.text = itemData.GetDescription(item.count);
    }

    public virtual void OnClickedGet()
    {
        GameEventBus.Publish<AddedItemEvent>(new AddedItemEvent(itemData));
        ItemCanvas.Instance.CloseCanvas();
    }
}