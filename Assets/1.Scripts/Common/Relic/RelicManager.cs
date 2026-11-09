using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RelicManager : MonoSingleton<RelicManager>
{
    public const string LABEL = "RelicData"; // Addressables 라벨

    public RelicData[] relicDatas;
    public Dictionary<string, RelicData> relicDataDic = new Dictionary<string, RelicData>();
    public UniTask LoadTask { get; private set; }

    void Awake()
    {
        LoadTask = LoadDataAsync().Preserve();
    }

    async UniTask LoadDataAsync()
    {
        await AddressableMgr.LoadAllByLabel<RelicData>(LABEL, (datas) =>
        {
            relicDatas = datas;
            foreach (RelicData relicData in relicDatas)
            {
                relicDataDic[relicData.key] = relicData;
            }
        });
    }

    public RelicData GetRelicData(string key)
    {
        return relicDataDic.TryGetValue(key, out RelicData data) ? data : null;
    }

    // 묶음(1~6)의 유물 3개
    public List<RelicData> GetTierRelics(int tier)
    {
        return relicDatas.Where(r => r.tier == tier).OrderBy(r => r.key).ToList();
    }

    // 로비에 장착된 슬라임들의 유형 태그 (유물 해당 종 수 계산용)
    public static List<SlimeTag> GetEquippedSlimeTags()
    {
        return UserDataManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes
            .Where(s => s != null && s.SlimeData != null)
            .Select(s => s.SlimeData.tags)
            .ToList();
    }
}
