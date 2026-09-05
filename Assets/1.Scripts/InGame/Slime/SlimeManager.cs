using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

public class SlimeManager : MonoSingleton<SlimeManager>
{
    public Dictionary<string, SlimeData> slimeDataDic = new Dictionary<string, SlimeData>();
    public Dictionary<GradeType, List<string>> gradeGroupGrowth1SlimeDic = new Dictionary<GradeType, List<string>>();
    public SlimeData[] allSlimeDatas;
    public SlimeData growth0SlimeData;
    public SlimeData[] growth1SlimeDatas;
    public SlimeData[] growth2SlimeDatas;

    public Dictionary<GradeType, EnhanceGradeInfo> enhanceGradeInfoDic = new Dictionary<GradeType, EnhanceGradeInfo>();
    public Dictionary<GradeType, EnhanceExpInfo> enhanceExpInfoDic = new Dictionary<GradeType, EnhanceExpInfo>();
    public SlimeMergeData[] slimeMergeDatas;

    public Dictionary<string, SlimeMergeData> slimeMergeDataDic = new Dictionary<string, SlimeMergeData>();

    public UniTask LoadTask { get; private set; }

    void Awake()
    {
        LoadTask = LoadDataAsync();
    }

    async UniTask LoadDataAsync()
    {
        await AddressableMgr.LoadAllByLabel<SlimeData>("SlimeData", (dates) =>
        {
            allSlimeDatas = dates;
            foreach (SlimeData slimeData in allSlimeDatas)
            {
                slimeDataDic[slimeData.key] = slimeData;
            }

        });

        await AddressableMgr.LoadAllByLabel<SlimeData>("Growth0SlimeData", (dates) =>
        {
            growth0SlimeData = dates[0];

        });
        await AddressableMgr.LoadAllByLabel<SlimeData>("Growth1SlimeData", (dates) =>
        {
            growth1SlimeDatas = dates;

            foreach (SlimeData slimeData in growth1SlimeDatas)
            {
                if (!gradeGroupGrowth1SlimeDic.ContainsKey(slimeData.grade))
                {
                    gradeGroupGrowth1SlimeDic.Add(slimeData.grade, new List<string>());
                }
                gradeGroupGrowth1SlimeDic[slimeData.grade].Add(slimeData.key);

            }

        });
        await AddressableMgr.LoadAllByLabel<SlimeData>("Growth2SlimeData", (dates) =>
        {
            growth2SlimeDatas = dates;

        });

        await AddressableMgr.LoadAllByLabel<SlimeMergeData>("SlimeMergeData", (dates) =>
        {
            slimeMergeDatas = dates;

            for (int i = 0; i < slimeMergeDatas.Length; i++)
            {
                slimeMergeDataDic.Add(slimeMergeDatas[i].key, slimeMergeDatas[i]);
            }


        });

        await AddressableMgr.LoadAllByLabel<EnhanceGradeInfo>("EnhanceGradeInfo", (dates) =>
        {
            foreach (EnhanceGradeInfo info in dates)
            {
                enhanceGradeInfoDic[info.grade] = info;
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

    public SlimeData GetSlimeData(string key)
    {
        return slimeDataDic[key];
    }

    public SlimeMergeData GetSlimeMergeData(string key)
    {
        return slimeMergeDataDic[key];
    }
    public int GetEnhanceExpInfo(int lv, GradeType grade)
    {
        return GetEnhanceExpInfo(grade).exps[lv];
    }

    public EnhanceExpInfo GetEnhanceExpInfo(GradeType grade)
    {
        return enhanceExpInfoDic.TryGetValue(grade, out EnhanceExpInfo info) ? info : null;
    }

    public EnhanceGradeInfo GetEnhanceGradeInfo(GradeType grade)
    {
        return enhanceGradeInfoDic.TryGetValue(grade, out EnhanceGradeInfo info) ? info : null;
    }

    public List<SlimeMergeData> GetSlimeMergeDatas(string[] slimeKeys)
    {
        List<SlimeMergeData> canMakeSlimes = new List<SlimeMergeData>();
        SlimeMergeData[] conditionDatas = slimeMergeDatas;
        for (int i = 0; i < conditionDatas.Length; i++)
        {
            string[] requireKeys = conditionDatas[i].growth1SlimeKeys;
            List<string> pool = new List<string>(slimeKeys);
            bool canMake = true;
            foreach (string requireKey in requireKeys)
            {
                string matched = pool.FirstOrDefault(slimeKey => slimeKey == requireKey);
                if (matched == null)
                {
                    canMake = false;
                    break;
                }
                pool.Remove(matched);
            }

            if (canMake)
                canMakeSlimes.Add(conditionDatas[i]);
        }
        return canMakeSlimes;
    }
}
