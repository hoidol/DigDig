using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
// 난이도(Normal/Hard/Hell)마다 스테이지 하나. key = 난이도 이름
public class StageManager : MonoSingleton<StageManager>, ILoadData
{
    public StageData[] allStageDatas;
    public Dictionary<string,StageData> stageDataDict = new Dictionary<string, StageData>();
    public StageData normalStageData;
    public StageData hardStageData;
    public StageData hellStageData;

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

            normalStageData = dates.FirstOrDefault(e => e.difficulty == DifficultyType.Normal);
            hardStageData = dates.FirstOrDefault(e => e.difficulty == DifficultyType.Hard);
            hellStageData = dates.FirstOrDefault(e => e.difficulty == DifficultyType.Hell);
        });
    }

    // public StageData GetStageData(string key)
    // {
    //     stageDataDict.TryGetValue(key, out StageData stageData);
    //     return stageData;
    // }
    public StageData GetStageData(DifficultyType difficultyType)
    {
        if(difficultyType == DifficultyType.Normal)
        {
            return normalStageData;
        }else if(difficultyType == DifficultyType.Hard)
        {
            return hardStageData;
        }else if(difficultyType == DifficultyType.Hell)
        {
            return hellStageData;
        }
        return null;
    }

}
