using System;
using UnityEngine;
using UnityEngine.UI;

public class ChangeItemCanvas : CanvasUI<ChangeItemCanvas>
{
    [SerializeField] ItemDisplayPanel newItemDisplayPanel;
    [SerializeField] ItemDisplayPanel[] itemDisplayPanels;
    ItemData newItemData;
    [SerializeField] RectTransform selectIndicatorRect;

    void Awake()
    {
        for (int i = 0; i < itemDisplayPanels.Length; i++)
        {
            ItemDisplayPanel panel = itemDisplayPanels[i]; // 캡처용 변수
            Button btn = panel.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                SelectItem(panel);  // btn 대신 panel 직접 전달
            });
        }
    }
    ItemDisplayPanel selectedPanel;
    public void SelectItem(ItemDisplayPanel panel)
    {
        selectedPanel = panel;
        selectIndicatorRect.gameObject.SetActive(true);
        selectIndicatorRect.transform.position = selectedPanel.transform.position;
    }

    //교체 버튼 눌렀을때
    public void OnClickedChanged()
    {
        if (selectedPanel == null)
        {
            return;
        }
        //selectedPanel.itemData
        Player.Instance.itemInventory.ReleaseItem(selectedPanel.itemData.key);
        CloseCanvas();
    }

    public void OpenCanvas(ItemData newItemData, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        selectIndicatorRect.gameObject.SetActive(false);
        selectedPanel = null;
        this.newItemData = newItemData;
        newItemDisplayPanel.SetItemData(newItemData, 1);
        for (int i = 0; i < itemDisplayPanels.Length; i++)
        {
            if (i < Player.Instance.itemInventory.equippedItems.Count)
            {
                itemDisplayPanels[i].gameObject.SetActive(true);
                Item item = Player.Instance.itemInventory.equippedItems[i];
                itemDisplayPanels[i].SetItemData(item.itemData, item.count);
            }
            else
            {
                itemDisplayPanels[i].gameObject.SetActive(false);
            }
        }
    }

}
