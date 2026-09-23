using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
public class StageManager : MonoSingleton<StageManager>, ILoadData
{
    public StageData[] allStageDatas;
    public Dictionary<string,StageData> stageDataDict = new Dictionary<string, StageData>();
    public StageData[] normalStageDatas;
    public StageData[] hardStageDatas;
    public StageData[] hellStageDatas;    

    public UniTask LoadTask { get; private set; }

    void Awake()
    {
        LoadTask = LoadDataAsync().Preserve();
    }

    async UniTask LoadDataAsync()
    {
        Debug.Log("StageManager Awake ()호출되는지 확인");
        await AddressableMgr.LoadAllByLabel<StageData>("StageData", (dates) =>
        {
            allStageDatas = dates;
            for(int i = 0; i < allStageDatas.Length; i++)
            {
                stageDataDict.Add(allStageDatas[i].key,allStageDatas[i]);
            }

            normalStageDatas = dates.Where(e=>e.difficulty == DifficultyType.Normal).ToArray();
            hardStageDatas = dates.Where(e=>e.difficulty == DifficultyType.Hard).ToArray();
            hellStageDatas = dates.Where(e=>e.difficulty == DifficultyType.Hell).ToArray();
            
            normalStageDatas = normalStageDatas.OrderBy(e => e.order).ToArray();
            hardStageDatas = hardStageDatas.OrderBy(e => e.order).ToArray();
            hellStageDatas = hellStageDatas.OrderBy(e => e.order).ToArray();

        });
    }

    public StageData GetStageData(string key)
    {
        return stageDataDict[key];
    }
    public StageData[] GetStageDatas(DifficultyType difficultyType)
    {
        if(difficultyType == DifficultyType.Normal)
        {
            return normalStageDatas;
        }else if(difficultyType == DifficultyType.Hard)
        {
            return hardStageDatas;
        }else if(difficultyType == DifficultyType.Hell)
        {
            return hellStageDatas;
        }
        return null;
    }
    
    public StageData GetStageData(DifficultyType difficultyType,int order)
    {
        return GetStageDatas(difficultyType).Where(e => e.order == order).FirstOrDefault();
    }
}