using System.Collections.Generic;
using UnityEngine;

public class ItemListContainer : MonoBehaviour
{
    public ItemNotifyEffectPanel[] itemNotifyEffectPanels;
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
        List<string> itemKeys = Player.Instance.itemInventory.GetItemKeys();
        for (int i = 0; i < itemNotifyEffectPanels.Length; i++)
        {
            if (i < itemKeys.Count)
            {

                itemNotifyEffectPanels[i].SetItemData(ItemData.GetItemData(itemKeys[i]));
                itemNotifyEffectPanels[i].gameObject.SetActive(true);
            }
            else
            {
                itemNotifyEffectPanels[i].gameObject.SetActive(false);
            }

        }
    }
}
