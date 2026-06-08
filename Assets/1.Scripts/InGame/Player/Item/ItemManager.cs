using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoSingleton<ItemManager>
{
    public Dictionary<string, ItemData> itemDataDic = new Dictionary<string, ItemData>();
    public MergeItemData[] mergeItemDatas;
    public Dictionary<string, MergeItemData> mergeItemDataDic = new Dictionary<string, MergeItemData>();
    public ItemData[] itemDatas;
    void Awake()
    {
        itemDatas = Resources.LoadAll<ItemData>("ItemData");
        foreach (ItemData itemData in itemDatas)
        {
            itemDataDic[itemData.key] = itemData;
        }

        mergeItemDatas = Resources.LoadAll<MergeItemData>("MergeItemData");
        for (int i = 0; i < mergeItemDatas.Length; i++)
        {
            mergeItemDataDic.Add(mergeItemDatas[i].resultItemKey, mergeItemDatas[i]);
        }
    }
    public MergeItemData GetMergeItemData(string key)
    {
        if (!itemDataDic.ContainsKey(key))
            return null;
        return mergeItemDataDic[key];
    }

    public List<ItemData> GetDrawItems(int count)
    {
        //조합할 수 있는 아이템이 나올 확률 올리기
        var result = new List<ItemData>();
        for (int i = 0; i < itemDatas.Length; i++)
        {
            if (itemDatas[i].CheckUnlock())
                continue;

            if (Player.Instance.itemInventory.curItems.Any(e => e.key == itemDatas[i].key))
            {
                continue;
            }
            result.Add(itemDatas[i]);
        }
        return result.OrderBy(i => Random.value).Take(count).ToList();
    }
    class ItemCounter
    {
        public string key;
        public int count;
    }
    struct ItemPickChance
    {
        public ItemData itemData;
        public float chance;
    }
    public ItemData GetItemData(string key)
    {
        if (!itemDataDic.ContainsKey(key))
        {
            Debug.Log($"<color=#FF0000>Key 아이템 없음 {key}</color>");
            return null;
        }
        return itemDataDic[key];
    }

    public MergeItemData GetMergeItemData(params string[] itemKeys)
    {
        return mergeItemDatas.FirstOrDefault(d =>
            itemKeys.All(key => d.resourceItemKeys.Contains(key)) &&
            d.resourceItemKeys.Length == itemKeys.Length);
    }


}


public class TryPurchaseItemEvent
{
    public ItemData itemData;
    public TryPurchaseItemEvent(ItemData iData)
    {
        itemData = iData;
    }
}
public class NearDropItemEvent
{
    public ItemData itemData;
    public NearDropItemEvent(ItemData iData)
    {
        itemData = iData;
    }
}

public class TryMergeItemEvent
{
    public MergeItemData mergeItemData;

    public TryMergeItemEvent(MergeItemData data)
    {
        mergeItemData = data;
    }
}


public class MergedItemEvent
{
    public Item resourceItem1;
    public Item resourceItem2;
    public ItemData resultItemData;
    public MergedItemEvent(Item i1, Item i2, ItemData resultItemData)
    {
        resourceItem1 = i1;
        resourceItem2 = i2;
        this.resultItemData = resultItemData;
    }
}
