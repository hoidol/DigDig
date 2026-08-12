using UnityEngine;
using UnityEngine.UI;
public class ItemPanelOnlyImage : MonoBehaviour
{
    // public Image bgImage;
    [SerializeField] protected ItemData itemData;
    public Image thumImage;
    public virtual void SetItemData(ItemData itemData)
    {
        this.itemData = itemData;
        if(itemData != null)
        {
            thumImage.sprite = itemData.thum;
            thumImage.color = itemData.color;
            thumImage.enabled = true;
        }
        else
        {
            
            thumImage.enabled = false;
        }
        
    }
}
