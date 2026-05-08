
using UnityEngine;
using UnityEngine.UI;

public class ItemDisplayPanel : MonoBehaviour
{
    public Image thumImage;
    public Image bgImage;

    public ItemData itemData
    {
        get;
        private set;
    }
    public virtual void SetItemData(ItemData itemData)
    {
        bgImage.color = ItemData.GetGradeColor(itemData.grade);
        thumImage.sprite = itemData.thumbnail;
    }


}