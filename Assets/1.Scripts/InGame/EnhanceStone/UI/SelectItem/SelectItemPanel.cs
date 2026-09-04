using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectItemPanel : MonoBehaviour
{
    [SerializeField] protected ItemData itemData;

    public Image thumImage;
    public TMP_Text titleText;
    public TMP_Text descText;

    // public GameObject canMergePanel;
    // public ItemPanelOnlyImage[] itemPanelOnlyImages;

    // [SerializeField] List<MergeItemData> mergeItemDatas;
    public void SetItemData(ItemData itemData)
    {
        this.itemData = itemData;
        thumImage.sprite = itemData.thum;
        thumImage.color = itemData.color;

        titleText.text = itemData.Title;
        descText.text = itemData.GetDescription();

        //Check Merge 
        //mergeItemDatas.Clear();

        //mergeItemDatas = ItemManager.Instance.GetMergeItemDataList(itemData.key);
        //canMergePanel.SetActive(mergeItemDatas.Count > 0);
        // for (int i = 0; i < itemPanelOnlyImages.Length; i++)
        // {
        //     itemPanelOnlyImages[i].gameObject.SetActive(false);
        // }

        // for (int i = 0; i < mergeItemDatas.Count; i++)
        // {
        //     itemPanelOnlyImages[i].SetItemData(ItemData.GetItemData(mergeItemDatas[i].resultItemKey));
        // }

    }

    public void OnClickedButton()
    {

        GetComponentInParent<SelectItemCanvas>().CloseCanvas();
        Character.Instance.AddItem(itemData.key);


    }
}
