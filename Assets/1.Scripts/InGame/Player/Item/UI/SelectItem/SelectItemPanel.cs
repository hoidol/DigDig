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

    public GameObject canMergePanel;
    public ItemPanelOnlyImage[] itemPanelOnlyImages;

    [SerializeField] List<MergeItemData> mergeItemDatas;
    public void SetItemData(ItemData itemData)
    {
        this.itemData = itemData;
        thumImage.sprite = itemData.thum;
        thumImage.color = itemData.color;

        titleText.text = itemData.Title;
        descText.text = itemData.GetDescription();

        //Check Merge 
        mergeItemDatas.Clear();

        mergeItemDatas = ItemManager.Instance.GetMergeItemDataList(itemData.key);
        canMergePanel.SetActive(mergeItemDatas.Count > 0);
        for (int i = 0; i < itemPanelOnlyImages.Length; i++)
        {
            itemPanelOnlyImages[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < mergeItemDatas.Count; i++)
        {
            itemPanelOnlyImages[i].SetItemData(ItemData.GetItemData(mergeItemDatas[i].resultItemKey));
        }

    }

    public void OnClickedButton()
    {

        GetComponentInParent<SelectItemCanvas>().CloseCanvas();
        if (mergeItemDatas.Count > 1)
        {
            string havingItem = mergeItemDatas[0].childItemKeys.Where(e => e != itemData.key).FirstOrDefault();
            Player.Instance.AddItem(havingItem, -1);
            SelectMergeItemCanvas.Instance.OpenCanvas(mergeItemDatas, () =>
            {


            });
        }
        else if (mergeItemDatas.Count == 1)
        {
            // mergeItemDatas[0], itemData.key가 아닌 다른 아이템 제거하기
            string havingItem = mergeItemDatas[0].childItemKeys.Where(e => e != itemData.key).FirstOrDefault();
            Player.Instance.AddItem(havingItem, -1);
            Player.Instance.AddItem(mergeItemDatas[0].resultItemKey, 1);
        }
        else
        {
            Player.Instance.AddItem(itemData.key, 1);
        }


    }
}
