using UnityEngine;
using System.Linq;
using System.Collections.Generic;
public class StageManager : MonoSingleton<StageManager>
{
    public StageData[] stageDatas;
    public Dictionary<string, StageData> stageDict= new Dictionary<string, StageData>();
    void Awake()
    {
        stageDatas = Resources.LoadAll<StageData>("StageData");
        stageDatas = stageDatas.OrderBy(e=>e.order).ToArray();

        for(int i = 0; i < stageDatas.Length; i++)
        {
            stageDict.Add(stageDatas[i].key,stageDatas[i]); 
        }
    }

    public StageData GetStageData(string key)
    {
        return stageDict[key];
    }
    public StageData GetStageData(int order)
    {
        return stageDatas.Where(e=>e.order == order).FirstOrDefault();
    }
}