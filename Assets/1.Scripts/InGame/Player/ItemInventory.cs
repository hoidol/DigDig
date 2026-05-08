using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ItemInventory : MonoBehaviour
{
    public List<Item> equippedItems = new List<Item>();
    public List<MergeItemData> canMergeItemDatas = new List<MergeItemData>();
    //public readonly int MAX_ITEM_COUNT = 8;

    // 인터페이스별 캐시 - 장착/해제 시점에만 갱신
    public List<IPreAttack> preAttackItems = new List<IPreAttack>();
    public List<IAttackItem> attackItems = new List<IAttackItem>();
    public List<IComboAttackItem> comboAttackItems = new List<IComboAttackItem>();
    public List<IBulletItem> bulletItems = new List<IBulletItem>();

    void RefreshCache()
    {
        preAttackItems = equippedItems.OfType<IPreAttack>().ToList();
        attackItems = equippedItems.OfType<IAttackItem>().ToList();
        comboAttackItems = equippedItems.OfType<IComboAttackItem>().ToList();
        bulletItems = equippedItems.OfType<IBulletItem>().ToList();
    }

    void Awake()
    {
        GameEventBus.Subscribe<TryAddItemEvent>(TryAddItemEvent);
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
        for (int i = 0; i < equippedItems.Count; i++)
        {
            totalCount += equippedItems[i].count;
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
        for (int i = 0; i < Player.Instance.itemInventory.equippedItems.Count; i++)
        {
            for (int j = 0; j < Player.Instance.itemInventory.equippedItems[i].count; j++)
            {
                itemKeys.Add(Player.Instance.itemInventory.equippedItems[i].key);
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
        Debug.Log($"{itemData.key} 아이템 장착하기");
        Item item = equippedItems.FirstOrDefault(e => e.key == itemData.key);
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
            equippedItems.Add(item);
        }

        GameEventBus.Publish(new AddedItemEvent(itemData));
        SortingItem();
        CheckMerge();
    }

    public void SortingItem()
    {
        equippedItems = equippedItems.OrderBy(e => e.itemData.applyOrder).ToList();
        RefreshCache();
    }

    public void ReleaseItem(string key)
    {
        Item item = equippedItems.FirstOrDefault(e => e.key == key);
        item.count--;
        if (item.count == 0)
        {
            item.OnUnequip(Player.Instance);
            equippedItems.Remove(item);
            Destroy(item.gameObject);
        }
        SortingItem(); // RefreshCache 포함
        CheckMerge();
    }

    public Item GetItem(string key)
    {
        return equippedItems.FirstOrDefault(e => e.key == key);
    }

    public void CheckMerge()
    {
        canMergeItemDatas.Clear();
        MergeItemData[] mergeItemDatas = ItemManager.Instance.mergeItemDatas;

        var mergeableItemKeys = new HashSet<string>(
            equippedItems.Where(item => item.CanMerge()).Select(item => item.key)
        );

        var ownedSkillKeys = new HashSet<string>(
            Player.Instance.abilityInventory.equippedAbilitys.Select(s => s.key)
        );

        foreach (var m in mergeItemDatas)
        {
            bool itemsOk = m.resourceItemKeys.All(k => mergeableItemKeys.Contains(k));
            bool abilityOk = m.resourceAbilityKeys == null || m.resourceAbilityKeys.All(k => ownedSkillKeys.Contains(k));
            if (itemsOk && abilityOk)
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
