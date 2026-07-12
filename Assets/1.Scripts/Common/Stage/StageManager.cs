using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
public class StageManager : MonoSingleton<StageManager>, ILoadData
{
    public StageData[] stageDatas;
    public Dictionary<string, StageData> stageDict = new Dictionary<string, StageData>();

    public UniTask LoadTask { get; private set; }

    void Awake()
    {
        LoadTask = LoadDataAsync();
    }

    async UniTask LoadDataAsync()
    {
        Debug.Log("StageManager Awake ()호출되는지 확인");
        await AddressableMgr.LoadAllByLabel<StageData>("StageData", (dates) =>
        {
            stageDatas = dates;
            stageDatas = stageDatas.OrderBy(e => e.order).ToArray();

            for (int i = 0; i < stageDatas.Length; i++)
            {
                stageDict.Add(stageDatas[i].key, stageDatas[i]);
            }

        });
    }

    public StageData GetStageData(string key)
    {
        if (!stageDict.ContainsKey(key))
            return Resources.Load<StageData>($"StageData/{key}");
        return stageDict[key];
    }
    public StageData GetStageData(int order)
    {
        return stageDatas.Where(e => e.order == order).FirstOrDefault();
    }
}