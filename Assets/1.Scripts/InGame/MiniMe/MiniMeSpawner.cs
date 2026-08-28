using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MiniMeSpawner : MonoSingleton<MiniMeSpawner>
{
    readonly Dictionary<string, Stack<MiniMe>> pool = new(); // 적 종류 별 풀링

    [field: SerializeField] public int ActiveMiniMeCount { get; private set; }

    public readonly HashSet<MiniMe> activeminiMes = new();
    public int miniMeSlotCount;
    void Start()
    {
        miniMeSlotCount = GameSetting.MIN_MINIME_SLOT_COUNT;
    }
    public void PurchaseSlot(int idx)
    {
        miniMeSlotCount++;   
    }
    int initPrice = 50;
    int increasePrice = 50;
    public int GetSlotPrice()
    {
        return  initPrice + increasePrice * (miniMeSlotCount -GameSetting.MIN_MINIME_SLOT_COUNT);
    }
    public MiniMe Instantiate(string key)
    {
        MiniMeData data = MiniMeManager.Instance.miniMeDataDic[key];

        if (!pool.ContainsKey(data.prefab.key))
            pool[data.prefab.key] = new Stack<MiniMe>();

        MiniMe miniMe = pool[data.prefab.key].Count > 0
            ? pool[data.prefab.key].Pop()
            : GameObject.Instantiate(data.prefab);


        miniMe.gameObject.SetActive(true);
        activeminiMes.Add(miniMe);
        ActiveMiniMeCount++;
        return miniMe;
    }
    

    public void Release(MiniMe miniMe)
    {
        if (!pool.ContainsKey(miniMe.key))
            pool[miniMe.key] = new Stack<MiniMe>();

        activeminiMes.Remove(miniMe);
        miniMe.gameObject.SetActive(false);
        pool[miniMe.key].Push(miniMe);
        ActiveMiniMeCount = Mathf.Max(0, ActiveMiniMeCount - 1);
    }

    public async UniTask<(bool, string, int)> Merge(MiniMe miniMe1, MiniMe miniMe2)
    {
        return await miniMe1.Merge(miniMe2);

        // if (miniMe1.MiniMeData.growth > 1 || miniMe2.MiniMeData.growth > 1)
        //     return (false, null,0);
        // if (miniMe1.MiniMeData.growth != miniMe2.MiniMeData.growth)
        //     return (false, null,0);
        // if (miniMe1.level != miniMe2.level)
        //     return (false, null,0);

        // string pickedMiniMeKey = null;
        // int lv = 0;
        // if (miniMe1.MiniMeData.growth == 1 && miniMe1.level == 2)
        // {
        //     pickedMiniMeKey = await SelectGrowup2MiniMeCanvas.Instance.OpenCanvas(miniMe1, miniMe2);
        //     if (pickedMiniMeKey == null)
        //         return (false, null,0);
        // }
        //  else if (miniMe1.MiniMeData.growth == 1&& miniMe1.level < 2)
        // {
        //     UserMiniMe userMiniMe = UserManager.Instance.userMiniMeManager.userMiniMeData.equiptedMiniMes[Random.Range(0, UserManager.Instance.userMiniMeManager.userMiniMeData.equiptedMiniMes.Length)];
        //     pickedMiniMeKey = userMiniMe.key;
        //     lv = miniMe1.level++;
        // }
        // else if (miniMe1.MiniMeData.growth == 0)
        // {
        //     UserMiniMe userMiniMe = UserManager.Instance.userMiniMeManager.userMiniMeData.equiptedMiniMes[Random.Range(0, UserManager.Instance.userMiniMeManager.userMiniMeData.equiptedMiniMes.Length)];
        //     pickedMiniMeKey = userMiniMe.key;
        // }

        // return (true, pickedMiniMeKey,lv);
    }


}
