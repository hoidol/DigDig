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
        thumImage.sprite = itemData.thum;
        thumImage.color = itemData.color;
        // bgImage.color = ItemData.GetGradeColor(itemData.grade);
        // thumImage.sprite = itemData.thumbnail;
    }
}
