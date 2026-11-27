using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RuneManager : MonoSingleton<RuneManager>, ILoadData
{
    public Dictionary<string, RuneData> runeDataDic = new Dictionary<string, RuneData>();
    public MergeRuneData[] mergeRuneDatas;
    public Dictionary<string, MergeRuneData> mergeRuneDataDic = new Dictionary<string, MergeRuneData>();
    public RuneData[] allRuneDatas;

    public RuneData[] level1RuneDatas;
    public RuneData[] level2RuneDatas;
    public UniTask LoadTask { get; private set; }

    void Awake()
    {
        LoadTask = LoadDataAsync().Preserve();
    }

    async UniTask LoadDataAsync()
    {
        await AddressableMgr.LoadAllByLabel<RuneData>("RuneData", (dates) =>
        {
            allRuneDatas = dates;
            foreach (RuneData runeData in allRuneDatas)
            {
                runeDataDic[runeData.key] = runeData;
            }

        });
        await AddressableMgr.LoadAllByLabel<RuneData>("Level1RuneData", (dates) =>
        {
            level1RuneDatas = dates;

        });
        await AddressableMgr.LoadAllByLabel<RuneData>("Level2RuneData", (dates) =>
        {
            level2RuneDatas = dates;

        });
    }

    public MergeRuneData GetMergeRuneData(string key)
    {
        if (!runeDataDic.ContainsKey(key))
            return null;
        return mergeRuneDataDic[key];
    }

    public List<RuneData> GetDrawRunes(int count)
    {
        //조합할 수 있는 아이템이 나올 확률 올리기
        var result = new List<RuneData>();
        for (int i = 0; i < level1RuneDatas.Length; i++)
        {
            if (!level1RuneDatas[i].CheckUnlock())
                continue;
            // //현재 보유중
            // if (Player.Instance.itemInventory.curItems.Any(e => e.key == level1ItemDatas[i].key))
            // {
            //     continue;
            // }

            if (Slime.Instance.statMgr.runeStatDic.ContainsKey(level1RuneDatas[i].key))
            {
                if (Slime.Instance.statMgr.runeStatDic[level1RuneDatas[i].key].count >= level1RuneDatas[i].maxCount)
                    continue;
            }

            //이미 Result안에 있음
            if (result.Any(e => e.key == level1RuneDatas[i].key))
                continue;

            result.Add(level1RuneDatas[i]);
        }
        return result.OrderBy(i => Random.value).Take(count).ToList();
    }

    // 뽑기용: TileRune 중 겹치지 않게 랜덤 count개 (해금 안 됨 / 최대 보유 개수 도달은 제외). 설치할 타일이 없으면 빈 리스트
    // avoid: 가능하면 빼고 뽑을 아이템 (갱신 시 지금 보이는 것). 후보가 모자라면 avoid에서 채움
    public List<RuneData> GetDrawTileRunes(int count, ICollection<RuneData> avoid = null)
    {
        if (TileManager.Instance.GetRandomTileForRune() == null)
            return new List<RuneData>();

        var candidates = allRuneDatas.Where(data =>
        {
            if (!(data.runePrefab is TileRune tileRune) || !tileRune.CanDraw() || !data.CheckUnlock())
                return false;
            if (!tileRune.IgnoreMaxCount && Slime.Instance.statMgr.runeStatDic.TryGetValue(data.key, out CharacterRuneStat stat) && stat.count >= data.maxCount)
                return false;
            return true;
        }).ToList();

        // 확률 등장 아이템(진화 타일): 판정 성공 → 맨 앞(확정 노출), 실패 → 후보에서 제외
        var guaranteed = new List<RuneData>();
        foreach (RuneData data in candidates.ToList())
        {
            float? chance = ((TileRune)data.runePrefab).DrawChance;
            if (chance == null)
                continue;
            candidates.Remove(data);
            if (Random.Range(0f, 100f) < chance.Value)
                guaranteed.Add(data);
        }

        return guaranteed.Concat(candidates.OrderBy(data => avoid != null && avoid.Contains(data)).ThenBy(data => Random.value))
            .Take(count).ToList();
    }

    // (2026-11-21) 레벨업 룬 카드 후보 = SlimeGrowthSetting.runeKeys 중 지금 나올 수 있는 룬 (CanDraw: 타일 고치기는 막힌 칸이 있을 때만)
    // 보유 수 제한 없음 (같은 룬 칸에 또 고르면 레벨 +1, 최대 Lv.3)
    public List<RuneData> GetLevelUpRunes(IEnumerable<string> keys)
    {
        var list = new List<RuneData>();
        if (keys == null)
            return list;
        foreach (string key in keys)
        {
            if (string.IsNullOrEmpty(key) || !runeDataDic.TryGetValue(key, out RuneData data))
                continue;
            if (data.runePrefab is TileRune tileRune && tileRune.NeedTile && tileRune.CanDraw() && data.CheckUnlock())
                list.Add(data);
        }
        return list;
    }

    public RuneData GetRuneData(string key)
    {
        if (!runeDataDic.ContainsKey(key))
        {
            Debug.Log($"<color=#FF0000>Key 아이템 없음 {key}</color>");
            return null;
        }
        return runeDataDic[key];
    }


    public bool CheckCanMerge(string runeKey)
    {
        List<MergeRuneData> mergeRuneList = mergeRuneDatas.Where(e => e.childRuneKeys.Contains(runeKey)).ToList();
        for (int i = 0; i < mergeRuneList.Count; i++)
        {
            Rune child1 = Slime.Instance.runeInventory.GetRune(mergeRuneList[i].childRuneKeys[0]);
            Rune child2 = Slime.Instance.runeInventory.GetRune(mergeRuneList[i].childRuneKeys[1]);

            if (child1 != null && child2 != null)
                return true;
        }
        return false;

    }


    public List<MergeRuneData> GetMergeRuneDataList(string runeKey)
    {
        List<MergeRuneData> mergeRuneDataList = mergeRuneDatas.Where(e => e.childRuneKeys.Contains(runeKey)).ToList();
        List<MergeRuneData> curMergeRuneDataList = new List<MergeRuneData>();
        for (int i = 0; i < mergeRuneDataList.Count; i++)
        {
            Rune child1 = Slime.Instance.runeInventory.GetRune(mergeRuneDataList[i].childRuneKeys[0]);
            Rune child2 = Slime.Instance.runeInventory.GetRune(mergeRuneDataList[i].childRuneKeys[1]);

            if (child1 != null && child2 != null)
                curMergeRuneDataList.Add(mergeRuneDataList[i]);
        }

        return curMergeRuneDataList;
    }

    public List<MergeRuneData> GetMergeRuneDataList()
    {
        return mergeRuneDatas.Where(e => e.childRuneKeys.All(key => Slime.Instance.runeInventory.GetRune(key) != null)).ToList();
    }


}

