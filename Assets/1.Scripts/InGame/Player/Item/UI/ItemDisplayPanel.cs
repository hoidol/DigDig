
using UnityEngine;
using UnityEngine.UI;

public class ItemDisplayPanel : MonoBehaviour
{
    public Image thumImage;
    public Image bgImage;
    public GameObject[] overlapCountImages;

    public ItemData itemData
    {
        get;
        private set;
    }
    public virtual void SetItemData(ItemData itemData, int count)
    {
        bgImage.color = ItemData.GetGradeColor(itemData.grade);
        thumImage.sprite = itemData.thumbnail;
        UpdateOverlapCount(itemData, count);
    }

    public virtual void UpdateOverlapCount(ItemData itemData, int count)
    {
        if (itemData.isUnique || itemData.grade >= Grade.Legend)
        {
            for (int i = 0; i < overlapCountImages.Length; i++)
            {
                overlapCountImages[i].transform.parent.gameObject.SetActive(false);
            }
            return;
        }

        for (int i = 0; i < overlapCountImages.Length; i++)
        {
            overlapCountImages[i].transform.parent.gameObject.SetActive(true);
            if (i < count)
            {
                overlapCountImages[i].SetActive(true);
            }
            else
            {
                overlapCountImages[i].SetActive(false);
            }
        }
    }
}