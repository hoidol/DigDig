using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

public class MinionManager : MonoSingleton<MinionManager>
{
    public Dictionary<string, MinionData> minionDataDic = new Dictionary<string, MinionData>();
    public Dictionary<GradeType, List<string>> gradeGroupGrowth1MinionDic = new Dictionary<GradeType, List<string>>();
    public MinionData[] allMinionDatas;
    // public MinionData growth0MinionData;
    // public MinionData[] growth1MinionDatas;
    // public MinionData[] growth2MinionDatas;

    public Dictionary<GradeType, EnhanceExpInfo> enhanceExpInfoDic = new Dictionary<GradeType, EnhanceExpInfo>();
    public MinionMergeData[] minionMergeDatas;

    public Dictionary<string, MinionMergeData> minionMergeDataDic = new Dictionary<string, MinionMergeData>();

    public UniTask LoadTask { get; private set; }

    void Awake()
    {
        LoadTask = LoadDataAsync().Preserve();
    }

    async UniTask LoadDataAsync()
    {
        await AddressableMgr.LoadAllByLabel<MinionData>("MinionData", (dates) =>
        {
            allMinionDatas = dates;
            foreach (MinionData minionData in allMinionDatas)
            {
                minionDataDic[minionData.key] = minionData;
                // 뽑기(DrawMinion)용 등급별 목록 (growth 1 슬라임만)
                if (minionData.growth != 1)
                    continue;
                if (!gradeGroupGrowth1MinionDic.ContainsKey(minionData.grade))
                    gradeGroupGrowth1MinionDic.Add(minionData.grade, new List<string>());
                gradeGroupGrowth1MinionDic[minionData.grade].Add(minionData.key);
            }

        });


        await AddressableMgr.LoadAllByLabel<MinionMergeData>("MinionMergeData", (dates) =>
        {
            minionMergeDatas = dates;

            for (int i = 0; i < minionMergeDatas.Length; i++)
            {
                minionMergeDataDic.Add(minionMergeDatas[i].key, minionMergeDatas[i]);
            }


        });


        await AddressableMgr.LoadAllByLabel<EnhanceExpInfo>("EnhanceExpInfo", (dates) =>
        {
            foreach (EnhanceExpInfo info in dates)
            {
                enhanceExpInfoDic[info.grade] = info;
            }
        });
    }

    public MinionData GetMinionData(string key)
    {
        return minionDataDic[key];
    }

    public MinionMergeData GetMinionMergeData(string key)
    {
        return minionMergeDataDic[key];
    }
    public int GetEnhanceExpInfo(int lv, GradeType grade)
    {
        return GetEnhanceExpInfo(grade).exps[lv];
    }

    public EnhanceExpInfo GetEnhanceExpInfo(GradeType grade)
    {
        return enhanceExpInfoDic.TryGetValue(grade, out EnhanceExpInfo info) ? info : null;
    }

    public List<MinionMergeData> GetMinionMergeDatas(string[] minionKeys)
    {
        List<MinionMergeData> canMakeMinions = new List<MinionMergeData>();
        MinionMergeData[] conditionDatas = minionMergeDatas;
        for (int i = 0; i < conditionDatas.Length; i++)
        {
            string[] requireKeys = conditionDatas[i].growth1MinionKeys;
            List<string> pool = new List<string>(minionKeys);
            bool canMake = true;
            foreach (string requireKey in requireKeys)
            {
                string matched = pool.FirstOrDefault(minionKey => minionKey == requireKey);
                if (matched == null)
                {
                    canMake = false;
                    break;
                }
                pool.Remove(matched);
            }

            if (canMake)
                canMakeMinions.Add(conditionDatas[i]);
        }
        return canMakeMinions;
    }
}
