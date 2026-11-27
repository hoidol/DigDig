using UnityEngine.Serialization;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RuneInventory : MonoBehaviour
{
    [FormerlySerializedAs("curItems")] public List<Rune> curRunes = new List<Rune>();

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
        // Character.Instance.AddRune("Armor");        
        // Character.Instance.AddRune("Bow").Forget();
        // Character.Instance.AddRune("Candle");
        // Character.Instance.AddRune("Clover");        
        // Character.Instance.AddRune("Bandage").Forget();
        // Character.Instance.AddRune("Feather").Forget();
        // Character.Instance.AddRune("Hone").Forget();
        // Character.Instance.AddRune("Mirror").Forget();
        // Character.Instance.AddRune("RedEye").Forget();
        // Character.Instance.AddRune("SkullCane").Forget();
        // Character.Instance.AddRune("Mushroom").Forget();
        // Character.Instance.AddRune("WoodSword").Forget();
    }
#endif

    // tile: 타일 아이템을 설치할 타일 (null이면 랜덤)
    public void AddRune(string key, Tile tile = null)
    {
        Debug.Log($"RuneInventory AddRune {key}");
        RuneData runeData = RuneData.GetRuneData(key);
        Rune rune = Instantiate(runeData.runePrefab, transform);
        rune.key = key; // 프리팹 key가 비어 있으면 UpdateInventory에서 itemStatDic에 없다고 바로 Destroy됨
        if (rune is TileRune tileRune)
            tileRune.tile = tile;
        rune.OnEquip();
        curRunes.Add(rune);

        GameEventBus.Publish(new AddedRuneEvent(runeData));
    }

    // idx를 모르면(-1) key로 슬롯을 찾아서 비움
    public void RemoveRune(TileRune tileRune)
    {
        tileRune.OnUnequip();
        Destroy(tileRune.gameObject);
        curRunes.Remove(tileRune);
    }


    public void UpdateInventory()
    {
        // itemStatDic에는 없는데 curItems에 있는 아이템 Destroy
        foreach (var rune in curRunes.Where(i => !Slime.Instance.statMgr.runeStatDic.ContainsKey(i.key)).ToList())
        {
            rune.OnUnequip();
            Destroy(rune.gameObject);
            curRunes.Remove(rune);
        }

        foreach (var rune in curRunes)
        {
            // 타일 아이템은 보유 개수만큼 객체를 따로 만든다 (객체마다 다른 타일에 설치)
            rune.UpdateRune();
        }
    }

    public Rune GetRune(string key)
    {
        return curRunes.FirstOrDefault(e => e.key == key);
    }

    public bool CheckOwn(string key)
    {
        return GetRune(key) != null; // 보유 개수가 0이 되면 UpdateInventory에서 객체가 제거됨
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("아이템 랜덤 얻기");
            // Player.Instance.AddRune(RuneManager.Instance.GetDrawRunes(1)[0].key);
        }

    }
#endif

}

public class UpdateMergeRecommendRuneEvent
{
    public List<MergeRuneData> recommendMergeRunes;
    public UpdateMergeRecommendRuneEvent(List<MergeRuneData> list)
    {
        recommendMergeRunes = list;
    }
}

[System.Serializable]
public class CharacterRune
{
    public string key;
    public int count;
}

public class AddedRuneEvent
{
    [FormerlySerializedAs("itemData")] public RuneData runeData;
    public AddedRuneEvent(RuneData runeData)
    {
        this.runeData = runeData;
    }
}
