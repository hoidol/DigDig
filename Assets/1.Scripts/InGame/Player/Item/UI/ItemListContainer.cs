using UnityEngine;

public class ItemListContainer : MonoBehaviour
{
    public ItemActivePanel[] itemActivePanels;
    void Start()
    {
        GameEventBus.Subscribe<AddedItemEvent>(AddedItemEvent);
        UpdateContainer();
    }

    void AddedItemEvent(AddedItemEvent e)
    {
        UpdateContainer();
    }

    void UpdateContainer()
    {
        for (int i = 0; i < itemActivePanels.Length; i++)
        {
            if (i < Player.Instance.itemInventory.equippedItems.Count)
            {
                itemActivePanels[i].SetItemData(Player.Instance.itemInventory.equippedItems[i].itemData, Player.Instance.itemInventory.equippedItems[i].count);
                itemActivePanels[i].gameObject.SetActive(true);
            }
            else
            {
                itemActivePanels[i].gameObject.SetActive(false);
            }

        }
    }
}
