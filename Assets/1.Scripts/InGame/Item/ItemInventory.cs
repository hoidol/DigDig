using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ItemInventory : MonoBehaviour
{
    public List<Item> curItems = new List<Item>();

    public readonly static int MAX_ITEM_COUNT = 4; // 최대 보유 아이템 개수 
    public string[] ownItemKeys = new string[MAX_ITEM_COUNT];

    // 인터페이스별 캐시 - 장착/해제 시점에만 갱신
    public List<IPreFire> preFires = new List<IPreFire>();
    public List<IFired> fireds = new List<IFired>();
    public List<IComboFire> comboFires = new List<IComboFire>();
    public List<IBullet> bullets = new List<IBullet>();

    void RefreshCache()
    {
        preFires = curItems.OfType<IPreFire>().ToList();
        fireds = curItems.OfType<IFired>().ToList();
        comboFires = curItems.OfType<IComboFire>().ToList();
        bullets = curItems.OfType<IBullet>().ToList();
    }

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
        // Character.Instance.AddItem("Armor");        
        Character.Instance.AddItem("Bow").Forget();
        // Character.Instance.AddItem("Candle");
        // Character.Instance.AddItem("Clover");        
        // Character.Instance.AddItem("Bandage").Forget();
        // Character.Instance.AddItem("Feather").Forget();
        // Character.Instance.AddItem("Hone").Forget();
        Character.Instance.AddItem("Mirror").Forget();
        // Character.Instance.AddItem("RedEye").Forget();
        Character.Instance.AddItem("SkullCane").Forget();
        // Character.Instance.AddItem("Mushroom").Forget();
        // Character.Instance.AddItem("WoodSword").Forget();
    }
#endif

    public void AddItem(string key)
    {
        for (int i = 0; i < ownItemKeys.Length; i++)
        {
            if (string.IsNullOrEmpty(ownItemKeys[i]))
            {
                ownItemKeys[i] = key;
                break;
            }
        }

        ItemData itemData = ItemData.GetItemData(key);
        GameEventBus.Publish(new AddedItemEvent(itemData));
    }

    // idx를 모르면(-1) key로 슬롯을 찾아서 비움
    public void RemoveItem(string key, int idx = -1)
    {
        if (idx < 0)
            idx = System.Array.LastIndexOf(ownItemKeys, key);
        if (idx < 0 || idx >= ownItemKeys.Length)
            return;

        ownItemKeys[idx] = null;
    }


    public void UpdateInventory()
    {
        // itemStatDic에는 없는데 curItems에 있는 아이템 Destroy
        foreach (var item in curItems.Where(i => !Character.Instance.statMgr.itemStatDic.ContainsKey(i.key)).ToList())
        {
            item.OnUnequip();
            Destroy(item.gameObject);
            curItems.Remove(item);
        }

        foreach (var data in Character.Instance.statMgr.itemStatDic)
        {
            Item item = GetItem(data.Value.key);
            if (item == null && data.Value.count > 0)
            {
                ItemData itemData = ItemData.GetItemData(data.Value.key);
                item = Instantiate(itemData.itemPrefab, transform);
                item.key = itemData.key;
                item.count = data.Value.count;
                curItems.Add(item);

                item.OnEquip();
                GameEventBus.Publish(new AddedItemEvent(itemData));
            }
            else if (item != null && data.Value.count == 0)
            {
                item.OnUnequip();
                Destroy(item.gameObject);
                curItems.Remove(item);
            }

            if (item != null)
                item.UpdateItem();
        }

        SortingItem();
    }

    public bool IsFull()
    {
        for (int i = 0; i < ownItemKeys.Length; i++)
        {
            if (string.IsNullOrEmpty(ownItemKeys[i]))
            {
                return false;
            }
        }
        return true;
    }

    // public int GetItemCount(string key)
    // {
    //     return ownItems.Count(e => e == key);
    // }

    public void SortingItem()
    {
        curItems = curItems.OrderBy(e => e.itemData.applyOrder).ToList();
        RefreshCache();
    }


    public Item GetItem(string key)
    {
        return curItems.FirstOrDefault(e => e.key == key);
    }

    public bool CheckOwn(string key)
    {
        return GetItem(key).count > 0;
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("아이템 랜덤 얻기");
            // Player.Instance.AddItem(ItemManager.Instance.GetDrawItems(1)[0].key);
        }

    }
#endif

}

public class UpdateMergeRecommendItemEvent
{
    public List<MergeItemData> recommendMergeItems;
    public UpdateMergeRecommendItemEvent(List<MergeItemData> list)
    {
        recommendMergeItems = list;
    }
}

[System.Serializable]
public class CharacterItem
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
