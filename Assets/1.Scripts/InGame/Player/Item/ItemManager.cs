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

    public List<ItemData> GetItems(Grade grade, int count)
    {
        //조합할 수 있는 아이템이 나올 확률 올리기
        List<string> canPickItemKeys = new List<string>();
        List<string> needItemDataForMerged = new List<string>(); //조합에 필요한 아이템 데이터
        List<ItemCounter> itemCounters = new();

        ItemCounter GetItemCounter(string key)
        {
            ItemCounter iCounter = itemCounters.FirstOrDefault(e => e.key == key);
            if (iCounter == null)
            {
                iCounter = new ItemCounter();
                iCounter.key = key;
                itemCounters.Add(iCounter);
            }

            return iCounter;
        }
        List<Item> items = Player.Instance.itemInventory.equippedItems;

        for (int i = 0; i < items.Count; i++)
        {
            ItemCounter itemCounter = GetItemCounter(items[i].key);
            itemCounter.count++;
            foreach (string mergeItemKey in items[i].itemData.mergeItemKeys)
            {
                MergeItemData mergeItemData = MergeItemData.GetMergeItemData(mergeItemKey);
                if (mergeItemData != null)
                {
                    foreach (string itemKey in mergeItemData.resourceItemKeys)
                    {
                        if (!needItemDataForMerged.Contains(itemKey))
                            needItemDataForMerged.Add(itemKey);
                    }
                }
            }
            foreach (string includingItemKey in items[i].itemData.IncludingItems)
            {
                itemCounter = GetItemCounter(includingItemKey);
                itemCounter.count++;
            }
        }

        var pool = itemDataDic.Values.Where(d =>
        {
            if ((d.acquireMethod & (AcquireMethod.Purchase | AcquireMethod.Select)) == 0)
                return false;
            ItemCounter itemCounter = GetItemCounter(d.key);
            if (itemCounter.count >= d.maxOwnCount)
                return false;
            return d.grade <= grade;
        }).ToList();

        var weightPool = new List<ItemPickChance>();
        foreach (var itemData in pool)
        {
            float weight = itemData.grade switch
            {
                Grade.Normal => 65f,
                Grade.Rare => 30f,
                Grade.Unique => 5f,
                _ => 0f,
            };
            if (needItemDataForMerged.Contains(itemData.key))
                weight *= 1.5f;
            if (weight <= 0f) continue;
            weightPool.Add(new ItemPickChance { itemData = itemData, chance = weight });
        }

        var result = new List<ItemData>();
        int pickCount = Mathf.Min(count, weightPool.Count);
        for (int i = 0; i < pickCount; i++)
        {
            float total = weightPool.Sum(e => e.chance);
            float roll = Random.Range(0f, total);
            float cumulative = 0f;
            for (int j = 0; j < weightPool.Count; j++)
            {
                cumulative += weightPool[j].chance;
                if (roll < cumulative)
                {
                    result.Add(weightPool[j].itemData);
                    weightPool.RemoveAt(j);
                    break;
                }
            }
        }
        return result;
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
