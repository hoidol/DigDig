using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ItemInventory : MonoBehaviour
{
    public List<Item> curItems = new List<Item>();
    public List<MergeItemData> canMergeItemDatas = new List<MergeItemData>();
    //public readonly int MAX_ITEM_COUNT = 8;

    // 인터페이스별 캐시 - 장착/해제 시점에만 갱신
    public List<IPreAttack> preAttacks = new List<IPreAttack>();
    public List<IAttack> attacks = new List<IAttack>();
    public List<IComboAttack> comboAttacks = new List<IComboAttack>();
    public List<IBullet> bullets = new List<IBullet>();

    void RefreshCache()
    {
        preAttacks = curItems.OfType<IPreAttack>().ToList();
        attacks = curItems.OfType<IAttack>().ToList();
        comboAttacks = curItems.OfType<IComboAttack>().ToList();
        bullets = curItems.OfType<IBullet>().ToList();
    }

    void Awake()
    {
    }

    void Start()
    {
        GameEventBus.Subscribe<TryAddItemEvent>(TryAddItemEvent);
#if UNITY_EDITOR
        GameEventBus.Subscribe<StartGameEvent>(OnStartGame);
#endif
    }
#if UNITY_EDITOR
    void OnStartGame(StartGameEvent e)
    {
        // AddItem("BladeOrbit");
        // AddItem("BladeOrbit");
        // AddItem("BrokenDrone");
        // AddItem("BrokenDrone");
        // AddItem("Shell");

    }
#endif

    void TryAddItemEvent(TryAddItemEvent e)
    {
        AddItem(e.itemData);
    }

    public bool CanAddItem(ItemData itemData, bool openRemoveItem = true)
    {
        Item item = GetItem(itemData.key);
        int totalCount = 0;
        for (int i = 0; i < curItems.Count; i++)
        {
            totalCount += curItems[i].count;
        }

        if (item == null)
        {
            if (openRemoveItem)
            {
                ChangeItemCanvas.Instance.OpenCanvas(itemData, () => { });
            }
            return false;
        }
        return true;
    }
    List<string> itemKeys = new List<string>();
    public List<string> GetItemKeys()
    {
        itemKeys.Clear();
        for (int i = 0; i < Player.Instance.itemInventory.curItems.Count; i++)
        {
            for (int j = 0; j < Player.Instance.itemInventory.curItems[i].count; j++)
            {
                itemKeys.Add(Player.Instance.itemInventory.curItems[i].key);
            }
        }
        return itemKeys;
    }

    public void AddItem(string key)
    {
        AddItem(ItemData.GetItemData(key));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemData"></param>
    /// <param name="openChangeItem"> 아이템 더이상 획득 못하면 교체창 열기</param>
    public void AddItem(ItemData itemData, bool openChangeItem = true)
    {
        // Debug.Log($"{itemData.key} 아이템 장착하기");
        Item item = curItems.FirstOrDefault(e => e.key == itemData.key);
        if (item != null)
        {
            item.count++;
            item.UpdateItem();
        }
        else
        {
            item = Instantiate(itemData.itemPrefab, transform);
            item.key = itemData.key;
            item.count = 1;
            item.OnEquip(Player.Instance);
            curItems.Add(item);
        }

        GameEventBus.Publish(new AddedItemEvent(itemData));
        SortingItem();
    }

    public void SortingItem()
    {
        curItems = curItems.OrderBy(e => e.itemData.applyOrder).ToList();
        RefreshCache();
    }

    public void ReleaseItem(string key)
    {
        Item item = curItems.FirstOrDefault(e => e.key == key);
        item.count--;
        if (item.count == 0)
        {
            item.OnUnequip(Player.Instance);
            curItems.Remove(item);
            Destroy(item.gameObject);
        }
        SortingItem(); // RefreshCache 포함
    }

    public Item GetItem(string key)
    {
        return curItems.FirstOrDefault(e => e.key == key);
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

public class AddedItemEvent
{
    public ItemData itemData;
    public AddedItemEvent(ItemData itemData)
    {
        this.itemData = itemData;
    }
}
