using UnityEngine;

public class ItemListContainer : MonoBehaviour
{
    public ItemActivePanel[] itemActivePanels;
    void Awake()
    {
        GameEventBus.Subscribe<AddedItemEvent>(AddedItemEvent);
    }

    void AddedItemEvent(AddedItemEvent e)
    {
        UpdateContainer();
    }

    void UpdateContainer()
    {
        for (int i = 0; i < itemActivePanels.Length; i++)
        {
            if (i < Player.Instance.inventory.equippedItems.Count)
            {
                itemActivePanels[i].SetItemData(Player.Instance.inventory.equippedItems[i].itemData, Player.Instance.inventory.equippedItems[i].count);
                itemActivePanels[i].gameObject.SetActive(true);
            }
            else
            {
                itemActivePanels[i].gameObject.SetActive(false);
            }

        }
    }
}
