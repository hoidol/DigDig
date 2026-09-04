using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SlimeSpawner : MonoSingleton<SlimeSpawner>
{
    readonly Dictionary<string, Stack<Slime>> pool = new(); // 적 종류 별 풀링

    [field: SerializeField] public int ActiveSlimeCount { get; private set; }

    public readonly HashSet<Slime> activeslimes = new();
    public int slimeSlotCount;
    void Start()
    {
        slimeSlotCount = GameSetting.MIN_SLIME_SLOT_COUNT;
    }
    public void PurchaseSlot(int idx)
    {
        slimeSlotCount++;
    }
    int initPrice = 50;
    int increasePrice = 50;
    public int GetSlotPrice()
    {
        return initPrice + increasePrice * (slimeSlotCount - GameSetting.MIN_SLIME_SLOT_COUNT);
    }
    public Slime Instantiate(string key)
    {
        SlimeData data = SlimeManager.Instance.slimeDataDic[key];

        if (!pool.ContainsKey(data.prefab.key))
            pool[data.prefab.key] = new Stack<Slime>();

        Slime slime = pool[data.prefab.key].Count > 0
            ? pool[data.prefab.key].Pop()
            : GameObject.Instantiate(data.prefab);


        slime.gameObject.SetActive(true);
        activeslimes.Add(slime);
        ActiveSlimeCount++;
        return slime;
    }
    
    public Slime LevelUp(Slime s)
    {
        string key = s.key;
        int level = s.level;
        Character.Instance.RemoveSlime(s);
        Slime slime = Character.Instance.AddSlime(key, level + 1);
        return slime;
    }

    public void Release(Slime slime)
    {
        if (!pool.ContainsKey(slime.key))
            pool[slime.key] = new Stack<Slime>();

        activeslimes.Remove(slime);
        slime.gameObject.SetActive(false);
        pool[slime.key].Push(slime);
        ActiveSlimeCount = Mathf.Max(0, ActiveSlimeCount - 1);
    }

    public async UniTask<(bool, string, int)> Merge(Slime slime1, Slime slime2)
    {
        return await slime1.Merge(slime2);

        // if (slime1.SlimeData.growth > 1 || slime2.SlimeData.growth > 1)
        //     return (false, null,0);
        // if (slime1.SlimeData.growth != slime2.SlimeData.growth)
        //     return (false, null,0);
        // if (slime1.level != slime2.level)
        //     return (false, null,0);

        // string pickedSlimeKey = null;
        // int lv = 0;
        // if (slime1.SlimeData.growth == 1 && slime1.level == 2)
        // {
        //     pickedSlimeKey = await SelectGrowup2SlimeCanvas.Instance.OpenCanvas(slime1, slime2);
        //     if (pickedSlimeKey == null)
        //         return (false, null,0);
        // }
        //  else if (slime1.SlimeData.growth == 1&& slime1.level < 2)
        // {
        //     UserSlime userSlime = UserManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes[Random.Range(0, UserManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes.Length)];
        //     pickedSlimeKey = userSlime.key;
        //     lv = slime1.level++;
        // }
        // else if (slime1.SlimeData.growth == 0)
        // {
        //     UserSlime userSlime = UserManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes[Random.Range(0, UserManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes.Length)];
        //     pickedSlimeKey = userSlime.key;
        // }

        // return (true, pickedSlimeKey,lv);
    }


}
