using UnityEngine;
using System.Collections.Generic;

public class MergePanel : MonoBehaviour
{
    [SerializeField] List<ItemDisplayPanel> itemDisplayPanels;

    public void SetMergeItemData(MergeItemData data)
    {
        for (int i = 0; i < itemDisplayPanels.Count; i++)
        {
            if (i < data.resourceItemKeys.Length)
            {
                string key = data.resourceItemKeys[i];
                Item item = Player.Instance.itemInventory.GetItem(key);
                if (item != null)
                    itemDisplayPanels[i].SetItemData(item.itemData, item.count);
                itemDisplayPanels[i].gameObject.SetActive(true);
            }
            else
            {
                itemDisplayPanels[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnClickedMerge()
    {
        MergeItemCanvas.Instance.CloseCanvas();
    }

    public void OnClickedClose()
    {
        MergeItemCanvas.Instance.CloseCanvas();
    }
}
