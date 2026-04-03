using UnityEngine;

public class MergePanel : MonoBehaviour
{

    [SerializeField] ItemDisplayPanel item1DisplayPanel;
    [SerializeField] ItemDisplayPanel item2DisplayPanel;

    public void SetMergeItemData(MergeItemData data)
    {
        Item item1 = Player.Instance.inventory.GetItem(data.resourceItemKeys[0]);
        Item item2 = Player.Instance.inventory.GetItem(data.resourceItemKeys[1]);
        item1DisplayPanel.SetItemData(item1.itemData, item1.count);
        item1DisplayPanel.SetItemData(item2.itemData, item2.count);
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
