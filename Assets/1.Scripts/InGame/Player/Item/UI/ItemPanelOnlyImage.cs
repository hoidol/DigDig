using UnityEngine;
using UnityEngine.UI;
public class ItemPanelOnlyImage : MonoBehaviour
{
    public Image bgImage;
    public Image thumImage;
    public virtual void SetItemData(ItemData itemData)
    {
        bgImage.color = ItemData.GetGradeColor(itemData.grade);
        thumImage.sprite = itemData.thumbnail;
    }
}
