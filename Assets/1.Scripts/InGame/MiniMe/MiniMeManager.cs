using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MiniMeManager : MonoSingleton<MiniMeManager>
{
    public Dictionary<string, MiniMeData> miniMeDataDic = new Dictionary<string, MiniMeData>();
    public Dictionary<Grade, List<string>> gradeGroupGrowth1MiniMeDic = new Dictionary<Grade, List<string>>();
    public MiniMeData[] allMiniMeDatas;
    public MiniMeData growth0MiniMeData;
    public MiniMeData[] growth1MiniMeDatas;
    public MiniMeData[] growth2MiniMeDatas;

    public MiniMeMergeData[] miniMeMergeDatas;

    public Dictionary<string, MiniMeMergeData> miniMeMergeDataDic = new Dictionary<string, MiniMeMergeData>();

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

        await AddressableMgr.LoadAllByLabel<MiniMeData>("Growth0MiniMeData", (dates) =>
        {
            growth0MiniMeData = dates[0];

        });
        await AddressableMgr.LoadAllByLabel<MiniMeData>("Growth1MiniMeData", (dates) =>
        {
            growth1MiniMeDatas = dates;
            
            foreach (MiniMeData miniMeData in growth1MiniMeDatas)
            {
                if (!gradeGroupGrowth1MiniMeDic.ContainsKey(miniMeData.grade))
                {
                    gradeGroupGrowth1MiniMeDic.Add(miniMeData.grade, new List<string>());
                }
                gradeGroupGrowth1MiniMeDic[miniMeData.grade].Add(miniMeData.key);

            }

        });
        await AddressableMgr.LoadAllByLabel<MiniMeData>("Growth2MiniMeData", (dates) =>
        {
            growth2MiniMeDatas = dates;

        });

        await AddressableMgr.LoadAllByLabel<MiniMeMergeData>("MiniMeMergeData", (dates) =>
        {
            miniMeMergeDatas = dates;

            for(int i = 0; i < miniMeMergeDatas.Length; i++)
            {
                miniMeMergeDataDic.Add(miniMeMergeDatas[i].key, miniMeMergeDatas[i]);
            }
            

        });
    }

    public MiniMeData GetMiniMeData(string key)
    {
        return miniMeDataDic[key];
    }

    public MiniMeMergeData GetMiniMeMergeData(string key)
    {
        return miniMeMergeDataDic[key];
    }
}