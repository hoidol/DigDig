using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Inventory : MonoBehaviour
{
    public List<Item> equippedItems = new List<Item>();
    //여기에 있는 아이템들 만들 수 있음
    public List<MergeItemData> canMergeItemDatas = new List<MergeItemData>();
    public readonly int MAX_ITEM_COUNT = 5;

    void Awake()
    {
        GameEventBus.Subscribe<TryAddItemEvent>(TryAddItemEvent);
    }
    void Start()
    {
#if UNITY_EDITOR
        AddItem("Sword");
        // AddItem("RapidFire");
        // AddItem("RapidFire");
        // AddItem("MultiShot");
        // AddItem("MultiShot");
        // AddItem("BounceBullet");
        // AddItem("PierceBullet");
        // AddItem("PierceBullet");
        // AddItem("PierceBullet");

#endif
    }
    void TryAddItemEvent(TryAddItemEvent e)
    {
        AddItem(e.itemData);
    }

    public bool CanAddItem(ItemData itemData, bool openRemoveItem = true)
    {
        Item item = GetItem(itemData.key);
        if (item == null)
        {
            if (equippedItems.Count >= MAX_ITEM_COUNT)
            {
                if (openRemoveItem)
                {
                    ChangeItemCanvas.Instance.OpenCanvas(itemData, () =>
                    {

                    });
                }
                return false;
            }
        }

        return true;
    }
    public void AddItem(string key)
    {
        AddItem(ItemData.GetItemData(key));
    }
    public void AddItem(ItemData itemData, bool openChangeItem = true)
    {
        if (!CanAddItem(itemData, openChangeItem))
        {
            return;
        }
        Debug.Log($"{itemData.key} 아이템 장착하기");
        Item item = equippedItems.Where(e => e.key == itemData.key).FirstOrDefault();
        if (item != null)
        {
            item.count++;
            item.UpdateItem(); //아이템 장착 중일 시 업데이트
        }
        else
        {
            item = Instantiate(itemData.itemPrefab, transform);
            item.key = itemData.key;
            item.count = 1;
            item.OnEquip(Player.Instance); //아이템 장착 
            equippedItems.Add(item);
        }

        GameEventBus.Publish<AddedItemEvent>(new AddedItemEvent(itemData));

        SortingItem();
        CheckMerge();
    }
    public void SortingItem() //적용 순서를 고려
    {
        equippedItems = equippedItems.OrderBy(e => e.itemData.applyOrder).ToList();
    }

    public void ReleaseItem(string key)
    {
        Item item = equippedItems.Where(e => e.key == key).FirstOrDefault();
        item.count--;
        if (item.count == 0)
        {
            equippedItems.Remove(item);
            Destroy(item.gameObject);
        }
        SortingItem();
        CheckMerge();
    }


    public Item GetItem(string key)
    {
        return equippedItems.Where((e) => e.key == key).FirstOrDefault();
    }

    public void CheckMerge()
    {
        canMergeItemDatas.Clear();
        MergeItemData[] mergeItemDatas = ItemManager.Instance.mergeItemDatas;

        // CanMerge 가능한 key Set으로 미리 추출 (Contains가 O(1))
        var mergeableKeys = new HashSet<string>(
            equippedItems.Where(item => item.CanMerge()).Select(item => item.key)
        );

        if (mergeableKeys.Count == 0) return;

        foreach (var m in mergeItemDatas)
        {
            // 모든 재료 key가 mergeableKeys에 있으면 머지 가능
            if (m.resourceItemKeys.All(key => mergeableKeys.Contains(key)))
                canMergeItemDatas.Add(m);
        }

        GameEventBus.Publish(new UpdaterMergeRecommendItemEvent(canMergeItemDatas));
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
public class PlayerItem
{
    public string key;
    public int count;
}