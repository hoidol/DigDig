using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SlimeInventory : MonoBehaviour
{
    public List<Slime> curSlimes = new List<Slime>();

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

    public void AddSlime(Slime me)
    {
        curSlimes.Add(me);
    }

    public void RemoveSlime(Slime slime, int idx = -1)
    {
        Destroy(slime.gameObject);
        curSlimes.Remove(slime);

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
public class CharacterSlimeData
{
    public string key;
    public int count;
}

public class AddedSlimeEvent
{
    public ItemData itemData;
    public AddedSlimeEvent(ItemData itemData)
    {
        this.itemData = itemData;
    }
}
