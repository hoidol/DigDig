using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
// 스테이지(order 0, 1, 2 … = 화면 스테이지 1, 2, 3) × 난이도(Normal/Hard/Hell) = StageData 하나. key = "{난이도}_{order}"
// 출시 때는 스테이지 1(order 0)만. 스테이지 추가 = StageData 에셋({난이도}_{order}.asset) + StageData.csv 행
public class StageManager : MonoSingleton<StageManager>, ILoadData
{
    public StageData[] allStageDatas;
    public Dictionary<string, StageData> stageDataDict = new Dictionary<string, StageData>();
    // 있는 스테이지 번호 (오름차순)
    public List<int> stageOrders = new List<int>();

    public UniTask LoadTask { get; private set; }

    void Awake()
    {
        LoadTask = LoadDataAsync().Preserve();
    }

    async UniTask LoadDataAsync()
    {
        await AddressableMgr.LoadAllByLabel<StageData>("StageData", (dates) =>
        {
            allStageDatas = dates;
            stageDataDict.Clear();
            for (int i = 0; i < allStageDatas.Length; i++)
            {
                if (!stageDataDict.TryAdd(allStageDatas[i].key, allStageDatas[i]))
                    Debug.LogWarning($"[StageManager] 중복 스테이지 무시: {allStageDatas[i].name} ({allStageDatas[i].key})");
            }
            stageOrders = allStageDatas.Select(e => e.order).Distinct().OrderBy(e => e).ToList();
        });
    }

    public StageData GetStageData(int order, DifficultyType difficultyType)
    {
        stageDataDict.TryGetValue(StageData.MakeKey(order, difficultyType), out StageData stageData);
        return stageData;
    }

    // 지금 선택한 스테이지·난이도 (UserDataManager.stageOrder / difficultyType)
    public StageData GetCurrentStageData() => GetStageData(UserDataManager.stageOrder, UserDataManager.difficultyType);

    public bool HasStage(int order) => stageOrders.Contains(order);

    // 그 스테이지에 있는 난이도들
    public IEnumerable<DifficultyType> GetDifficulties(int order)
    {
        return allStageDatas.Where(e => e.order == order).Select(e => e.difficulty).OrderBy(e => e);
    }

    // 현재 스테이지 번호에서 dir(-1 이전 / +1 다음) 방향의 스테이지. 없으면 -1
    public int GetNeighborStageOrder(int order, int dir)
    {
        int idx = stageOrders.IndexOf(order);
        if (idx < 0)
            return -1;
        idx += dir;
        return idx >= 0 && idx < stageOrders.Count ? stageOrders[idx] : -1;
    }
}
