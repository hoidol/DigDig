using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MiniMeInventory : MonoBehaviour
{
    public List<MiniMe> curMiniMes = new List<MiniMe>();

    void Awake()
    {
    }

    void Start()
    {
#if UNITY_EDITOR
        GameEventBus.Subscribe<StartGameEvent>(OnStartGame);
#endif
    }
#if UNITY_EDITOR
    void OnStartGame(StartGameEvent e)
    {

    }
#endif

    public void AddMiniMe(MiniMe me)
    {
        curMiniMes.Add(me);
    }

    public void RemoveMiniMe(MiniMe miniMe, int idx = -1)
    {
        Destroy(miniMe.gameObject);
        curMiniMes.Remove(miniMe);

    }


    public void UpdateInventory()
    {
        // // itemStatDic에는 없는데 curItems에 있는 아이템 Destroy
        // foreach (var item in curItems.Where(i => !Character.Instance.statMgr.itemStatDic.ContainsKey(i.key)).ToList())
        // {
        //     item.OnUnequip();
        //     Destroy(item.gameObject);
        //     curItems.Remove(item);
        // }

        // foreach (var data in Character.Instance.statMgr.itemStatDic)
        // {
        //     Item item = GetItem(data.Value.key);
        //     if (item == null && data.Value.count > 0)
        //     {
        //         ItemData itemData = ItemData.GetItemData(data.Value.key);
        //         item = Instantiate(itemData.itemPrefab, transform);
        //         item.key = itemData.key;
        //         curItems.Add(item);

        //         item.OnEquip();
        //         GameEventBus.Publish(new AddedItemEvent(itemData));
        //     }
        //     else if (item != null && data.Value.count == 0)
        //     {
        //         item.OnUnequip();
        //         Destroy(item.gameObject);
        //         curItems.Remove(item);
        //     }

        //     if (item != null)
        //         item.UpdateItem();
        // }

    }

}

public class UpdaterMergeRecommendItemEvent
{
    public List<MergeItemData> recommendMergeItems;
    public UpdaterMergeRecommendItemEvent(List<MergeItemData> list)
    {
        recommendMergeItems = list;
    }
}

[System.Serializable]
public class CharacterMiniMeData
{
    public string key;
    public int count;
}

public class AddedMiniMeEvent
{
    public ItemData itemData;
    public AddedMiniMeEvent(ItemData itemData)
    {
        this.itemData = itemData;
    }
}
