using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ItemPanel : ItemPanelOnlyImage
{

    [Header("비어있으면 설정안됨")]
    public TMP_Text titleText;
    [Header("비어있으면 설정안됨")]
    public TMP_Text descText;
    public GameObject mergePanel;
    public ItemPanelOnlyImage[] mergeItemPanels;

    public override void SetItemData(ItemData itemData)
    {
        base.SetItemData(itemData);

        if (titleText != null)
            titleText.text = itemData.Title;

        if (descText != null)
            descText.text = itemData.GetDescription();
    }
}
