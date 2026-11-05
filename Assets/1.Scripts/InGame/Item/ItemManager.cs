using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ItemManager : MonoSingleton<ItemManager>, ILoadData
{
    public Dictionary<string, ItemData> itemDataDic = new Dictionary<string, ItemData>();
    public MergeItemData[] mergeItemDatas;
    public Dictionary<string, MergeItemData> mergeItemDataDic = new Dictionary<string, MergeItemData>();
    public ItemData[] allItemDatas;

    public ItemData[] level1ItemDatas;
    public ItemData[] level2ItemDatas;
    public UniTask LoadTask { get; private set; }

    void Awake()
    {
        LoadTask = LoadDataAsync().Preserve();
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
            // //현재 보유중
            // if (Player.Instance.itemInventory.curItems.Any(e => e.key == level1ItemDatas[i].key))
            // {
            //     continue;
            // }

            if (KingSlime.Instance.statMgr.itemStatDic.ContainsKey(level1ItemDatas[i].key))
            {
                if (KingSlime.Instance.statMgr.itemStatDic[level1ItemDatas[i].key].count >= level1ItemDatas[i].maxCount)
                    continue;
            }

            //이미 Result안에 있음
            if (result.Any(e => e.key == level1ItemDatas[i].key))
                continue;

            result.Add(level1ItemDatas[i]);
        }
        return result.OrderBy(i => Random.value).Take(count).ToList();
    }

    // 뽑기용: TileItem 중 겹치지 않게 랜덤 count개 (해금 안 됨 / 최대 보유 개수 도달은 제외). 설치할 타일이 없으면 빈 리스트
    // avoid: 가능하면 빼고 뽑을 아이템 (갱신 시 지금 보이는 것). 후보가 모자라면 avoid에서 채움
    public List<ItemData> GetDrawTileItems(int count, ICollection<ItemData> avoid = null)
    {
        if (TileManager.Instance.GetRandomTileForItem() == null)
            return new List<ItemData>();

        return allItemDatas.Where(data =>
        {
            if (!(data.itemPrefab is TileItem tileItem) || !tileItem.CanDraw() || !data.CheckUnlock())
                return false;
            if (KingSlime.Instance.statMgr.itemStatDic.TryGetValue(data.key, out CharacterItemStat stat) && stat.count >= data.maxCount)
                return false;
            return true;
        }).OrderBy(data => avoid != null && avoid.Contains(data)).ThenBy(data => Random.value).Take(count).ToList();
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


    public bool CheckCanMerge(string itemKey)
    {
        List<MergeItemData> mergeItemList = mergeItemDatas.Where(e => e.childItemKeys.Contains(itemKey)).ToList();
        for (int i = 0; i < mergeItemList.Count; i++)
        {
            Item child1 = KingSlime.Instance.itemInventory.GetItem(mergeItemList[i].childItemKeys[0]);
            Item child2 = KingSlime.Instance.itemInventory.GetItem(mergeItemList[i].childItemKeys[1]);

            if (child1 != null && child2 != null)
                return true;
        }
        return false;

    }


    public List<MergeItemData> GetMergeItemDataList(string itemKey)
    {
        List<MergeItemData> mergeItemDataList = mergeItemDatas.Where(e => e.childItemKeys.Contains(itemKey)).ToList();
        List<MergeItemData> curMergeItemDataList = new List<MergeItemData>();
        for (int i = 0; i < mergeItemDataList.Count; i++)
        {
            Item child1 = KingSlime.Instance.itemInventory.GetItem(mergeItemDataList[i].childItemKeys[0]);
            Item child2 = KingSlime.Instance.itemInventory.GetItem(mergeItemDataList[i].childItemKeys[1]);

            if (child1 != null && child2 != null)
                curMergeItemDataList.Add(mergeItemDataList[i]);
        }

        return curMergeItemDataList;
    }

    public List<MergeItemData> GetMergeItemDataList()
    {
        return mergeItemDatas.Where(e => e.childItemKeys.All(key => KingSlime.Instance.itemInventory.GetItem(key) != null)).ToList();
    }


}

