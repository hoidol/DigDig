using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MiniMeManager : MonoSingleton<MiniMeManager>
{
    public Dictionary<string, MiniMeData> miniMeDataDic = new Dictionary<string, MiniMeData>();

    public MiniMeData[] allMiniMeDatas;
    public MiniMeData level0MiniMeData;
    public MiniMeData[] level1MiniMeDatas;
    public MiniMeData[] level2MiniMeDatas;
    public UniTask LoadTask { get; private set; }

    void Awake()
    {
        LoadTask = LoadDataAsync();
    }

    async UniTask LoadDataAsync()
    {
        await AddressableMgr.LoadAllByLabel<MiniMeData>("MiniMeData", (dates) =>
        {
            allMiniMeDatas = dates;
            foreach (MiniMeData miniMeData in allMiniMeDatas)
            {
                miniMeDataDic[miniMeData.key] = miniMeData;
            }

        });
        await AddressableMgr.LoadAllByLabel<MiniMeData>("Level1MiniMeData", (dates) =>
        {
            level1MiniMeDatas = dates;

        });
        await AddressableMgr.LoadAllByLabel<MiniMeData>("Level2MiniMeData", (dates) =>
        {
            level2MiniMeDatas = dates;

        });
    }

    public MiniMeData GetMiniMeData(string key)
    {
        return miniMeDataDic[key];
    }
}