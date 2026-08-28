using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OwnItemPanel : ItemPanel
{
    public Item item;
    int idx;
    public void SetItem(Item item, int idx)
    {
        this.idx =idx;
        if(item != null)
        {
            SetItemData(item.itemData);
        }
        else
        {
            
            SetItemData(null);
        }
        
        this.item = item;
    }
    public void OnClickedButton()
    {
        //능력치 확인 및 버릴 수 있음 창 뜨기
        ItemInfoCanvas.Instance.OpenCanvas(item,idx);
    }
}