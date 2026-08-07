using System.Collections.Generic;
using UnityEngine;

public class ItemListContainer : MonoBehaviour
{
    public ItemNotifyEffectPanel[] itemNotifyEffectPanels;
    void Start()
    {
        for (int i = 0; i < itemNotifyEffectPanels.Length; i++)
        {
            itemNotifyEffectPanels[i].gameObject.SetActive(false);
        }
        GameEventBus.Subscribe<AddedItemEvent>(AddedItemEvent);
        GameEventBus.Subscribe<StartGameEvent>(OnStartGameEvent);
    }
    void OnStartGameEvent(StartGameEvent e)
    {

        UpdateContainer();
    }
    void AddedItemEvent(AddedItemEvent e)
    {
        UpdateContainer();
    }

    void UpdateContainer()
    {
        List<Item> items = Character.Instance.itemInventory.curItems;
        for (int i = 0; i < itemNotifyEffectPanels.Length; i++)
        {
            if (i < items.Count)
            {

                itemNotifyEffectPanels[i].SetItem(items[i]);
                itemNotifyEffectPanels[i].gameObject.SetActive(true);
            }
            else
            {
                itemNotifyEffectPanels[i].gameObject.SetActive(false);
            }

        }
    }
}
