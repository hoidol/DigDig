using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectMergeItemPanel : MonoBehaviour
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
        GetComponentInParent<SelectMergeItemCanvas>().CloseCanvas();
        Character.Instance.AddItem(resultItemData.key,1);        
    }
}
