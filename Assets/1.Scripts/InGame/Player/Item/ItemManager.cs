using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ItemManager : MonoSingleton<ItemManager>, ILoadData
{
    public Dictionary<string, ItemData> itemDataDic = new Dictionary<string, ItemData>();
    // public MergeItemData[] mergeItemDatas;
    public Dictionary<string, MergeItemData> mergeItemDataDic = new Dictionary<string, MergeItemData>();
    public ItemData[] itemDatas;
    public UniTask LoadTask { get; private set; }

    void Awake()
    {
        LoadTask = LoadDataAsync();
    }

    async UniTask LoadDataAsync()
    {
        await AddressableMgr.LoadAllByLabel<ItemData>("ItemData", (dates) =>
        {
            itemDatas = dates;
            foreach (ItemData itemData in itemDatas)
            {
                itemDataDic[itemData.key] = itemData;
            }

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
        for (int i = 0; i < itemDatas.Length; i++)
        {
            if (!itemDatas[i].CheckUnlock())
                continue;
            //현재 보유중
            if (Player.Instance.itemInventory.curItems.Any(e => e.key == itemDatas[i].key))
            {
                continue;
            }
            //이미 Result안에 있음
            if (result.Any(e => e.key == itemDatas[i].key))
            {
                continue;
            }

            result.Add(itemDatas[i]);
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

