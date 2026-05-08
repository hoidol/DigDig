using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ItemPanel : ItemPanelOnlyImage
{
    [SerializeField] protected ItemData itemData;

    public TMP_Text titleText;
    public TMP_Text descText;
    public GameObject mergePanel;
    public ItemPanelOnlyImage[] mergeItemPanels;

    public override void SetItemData(ItemData itemData)
    {
        base.SetItemData(itemData);
        this.itemData = itemData;
        thumImage.sprite = itemData.thumbnail;
        titleText.text = itemData.Title;
        descText.text = itemData.GetDescription();
        if (itemData.mergeItemKeys.Length > 0)
        {
            mergePanel.SetActive(true);
            for (int i = 0; i < mergeItemPanels.Length; i++)
            {
                if (i < itemData.mergeItemKeys.Length)
                {
                    mergeItemPanels[i].gameObject.SetActive(true);
                    mergeItemPanels[i].SetItemData(ItemData.GetItemData(itemData.mergeItemKeys[i]));
                }
                else
                {
                    mergeItemPanels[i].gameObject.SetActive(false);
                }

            }
        }
        else
        {
            mergePanel.SetActive(false);
        }
    }
}
