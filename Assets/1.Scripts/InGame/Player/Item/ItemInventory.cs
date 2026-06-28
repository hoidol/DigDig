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



    public void AddItem(string key)  //**Player로 부터 호출받기
    {
        AddItem(ItemData.GetItemData(key));


    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemData"></param>
    /// <param name="openChangeItem"> 아이템 더이상 획득 못하면 교체창 열기</param>
    public void AddItem(ItemData itemData) //**Player로 부터 호출받기
    {
        Item item = Instantiate(itemData.itemPrefab, transform);
        item.key = itemData.key;
        curItems.Add(item);



        item.OnEquip(Player.Instance);
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
        item.OnUnequip(Player.Instance);
        curItems.Remove(item);
        Destroy(item.gameObject);
        SortingItem(); // RefreshCache 포함
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
            Player.Instance.AddItem(ItemManager.Instance.GetDrawItems(1)[0].key);
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
