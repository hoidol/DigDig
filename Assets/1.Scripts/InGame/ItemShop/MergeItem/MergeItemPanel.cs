using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MergeItemPanel : MonoBehaviour
{
    [SerializeField] protected MergeItemData mergeItemData;

    public Image thumImage;
    public TMP_Text titleText;
    public TMP_Text descText;

    ItemData resultItemData;
    public ItemPanel[] childItemPanels;
    
    public void SetMergeItemData(MergeItemData mergeItemData)
    {
        this.mergeItemData = mergeItemData;
        if(mergeItemData == null)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        resultItemData = ItemData.GetItemData(mergeItemData.resultItemKey);
        
        titleText.text = resultItemData.Title;
        descText.text = resultItemData.GetDescription();
        for(int i = 0; i < childItemPanels.Length; i++)
        {
            childItemPanels[i].SetItemData(ItemData.GetItemData(mergeItemData.childItemKeys[i]));
        }
    }

    public void OnClickedButton()
    {        
        GetComponentInParent<MergeItemCanvas>().CloseCanvas();
        
        for(int i = 0; i < mergeItemData.childItemKeys.Length; i++)
            Character.Instance.RemoveItem(mergeItemData.childItemKeys[i]);
        
        Character.Instance.AddItem(resultItemData.key,1);        
    }
}
