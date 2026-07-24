using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ItemInventory : MonoBehaviour
{
    public List<Item> curItems = new List<Item>();
    public List<MergeItemData> canMergeItemDatas = new List<MergeItemData>();
    public readonly int MAX_ITEM_COUNT = 7; // 최대 보유 아이템 개수 

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
        // AddItem("BladeOrbit");
        // AddItem("BladeOrbit");
        // AddItem("BrokenDrone");
        // AddItem("BrokenDrone");
        // AddItem("Shell");

    }
#endif


    public void UpdateInventory()
    {
        foreach ( var data in Player.Instance.statMgr.itemStatDic)
        {
            Item item = GetItem(data.Value.key); 
            if(item == null && data.Value.count > 0)
            {
                ItemData itemData = ItemData.GetItemData(data.Value.key);
                item = Instantiate(itemData.itemPrefab, transform);
                item.key = itemData.key;
                curItems.Add(item);

                item.OnEquip();
                GameEventBus.Publish(new AddedItemEvent(itemData));
            }
            else if(item != null && data.Value.count == 0)
            {
                item.OnUnequip();
                Destroy(item.gameObject);
                curItems.Remove(item);
            }

            if(item != null)
                item.UpdateItem();
        }

        SortingItem();
    }



    public void SortingItem()
    {
        curItems = curItems.OrderBy(e => e.itemData.applyOrder).ToList();
        RefreshCache();
    }


    public Item GetItem(string key)
    {
        return curItems.FirstOrDefault(e => e.key == key);
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
