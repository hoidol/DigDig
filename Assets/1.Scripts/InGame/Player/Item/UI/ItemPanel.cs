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
        titleText.text = itemData.Title;
        descText.text = itemData.GetDescription();
    }
}
