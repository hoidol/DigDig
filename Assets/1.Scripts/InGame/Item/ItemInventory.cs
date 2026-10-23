using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ItemInventory : MonoBehaviour
{
    public List<Item> curItems = new List<Item>();

    // public readonly static int MAX_ITEM_COUNT = 4; // 최대 보유 아이템 개수 
    // public List<string> ownItemKeys = new List<string>();

    // 인터페이스별 캐시 - 장착/해제 시점에만 갱신
    // public List<IPreFire> preFires = new List<IPreFire>();
    // public List<IFired> fireds = new List<IFired>();
    // public List<IComboFire> comboFires = new List<IComboFire>();
    // public List<IBullet> bullets = new List<IBullet>();
    // public List<Tile> setupTiles = new List<Tile>(); //아이템이 적용된 타일

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
        // Character.Instance.AddItem("Bow").Forget();
        // Character.Instance.AddItem("Candle");
        // Character.Instance.AddItem("Clover");        
        // Character.Instance.AddItem("Bandage").Forget();
        // Character.Instance.AddItem("Feather").Forget();
        // Character.Instance.AddItem("Hone").Forget();
        // Character.Instance.AddItem("Mirror").Forget();
        // Character.Instance.AddItem("RedEye").Forget();
        // Character.Instance.AddItem("SkullCane").Forget();
        // Character.Instance.AddItem("Mushroom").Forget();
        // Character.Instance.AddItem("WoodSword").Forget();
    }
#endif

    public void AddItem(string key)
    {
        // ownItemKeys.Add(key);

        ItemData itemData = ItemData.GetItemData(key);
        Item item = Instantiate(itemData.itemPrefab, transform);
        item.OnEquip();
        curItems.Add(item);
        
        GameEventBus.Publish(new AddedItemEvent(itemData));
    }

    // idx를 모르면(-1) key로 슬롯을 찾아서 비움
    public void RemoveItem(TileItem tileItem)
    {
        tileItem.OnUnequip();
        Destroy(tileItem.gameObject);
        curItems.Remove(tileItem);
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

        foreach (var item in curItems)
        {
            // 타일 아이템은 보유 개수만큼 객체를 따로 만든다 (객체마다 다른 타일에 설치)
            item.UpdateItem();
        }
    }

    public Item GetItem(string key)
    {
        return curItems.FirstOrDefault(e => e.key == key);
    }

    public bool CheckOwn(string key)
    {
        return GetItem(key) != null; // 보유 개수가 0이 되면 UpdateInventory에서 객체가 제거됨
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
