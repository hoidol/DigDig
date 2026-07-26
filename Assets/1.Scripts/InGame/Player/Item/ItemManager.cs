using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ItemManager : MonoSingleton<ItemManager>, ILoadData
{
    public Dictionary<string, ItemData> itemDataDic = new Dictionary<string, ItemData>();
    // public MergeItemData[] mergeItemDatas;
    public Dictionary<string, MergeItemData> mergeItemDataDic = new Dictionary<string, MergeItemData>();
    public ItemData[] allItemDatas;

    public ItemData[] level1ItemDatas;
    public ItemData[] level2ItemDatas;
    public UniTask LoadTask { get; private set; }

    void Awake()
    {
        LoadTask = LoadDataAsync();
    }

    async UniTask LoadDataAsync()
    {
        await AddressableMgr.LoadAllByLabel<ItemData>("ItemData", (dates) =>
        {
            allItemDatas = dates;
            foreach (ItemData itemData in allItemDatas)
            {
                itemDataDic[itemData.key] = itemData;
            }

        });
        await AddressableMgr.LoadAllByLabel<ItemData>("Level1ItemData", (dates) =>
        {
            level1ItemDatas = dates;

        });
        await AddressableMgr.LoadAllByLabel<ItemData>("Level2ItemData", (dates) =>
        {
            level2ItemDatas = dates;

        });
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
        for (int i = 0; i < level1ItemDatas.Length; i++)
        {
            if (!level1ItemDatas[i].CheckUnlock())
                continue;
            //현재 보유중
            if (Player.Instance.itemInventory.curItems.Any(e => e.key == level1ItemDatas[i].key))
            {
                continue;
            }
            //이미 Result안에 있음
            if (result.Any(e => e.key == level1ItemDatas[i].key))
            {
                continue;
            }

            result.Add(level1ItemDatas[i]);
        }
        return result.OrderBy(i => Random.value).Take(count).ToList();
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

    // public MergeItemData GetMergeItemData(params string[] itemKeys)
    // {
    //     return mergeItemDatas.FirstOrDefault(d =>
    //         itemKeys.All(key => d.resourceItemKeys.Contains(key)) &&
    //         d.resourceItemKeys.Length == itemKeys.Length);
    // }


}

