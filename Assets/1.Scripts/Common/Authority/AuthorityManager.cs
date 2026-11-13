using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AuthorityManager : MonoSingleton<AuthorityManager>
{
    public const string LABEL = "AuthorityData"; // Addressables 라벨

    public AuthorityData[] authorityDatas;
    public Dictionary<string, AuthorityData> authorityDataDic = new Dictionary<string, AuthorityData>();
    public UniTask LoadTask { get; private set; }

    void Awake()
    {
        LoadTask = LoadDataAsync().Preserve();
    }

    async UniTask LoadDataAsync()
    {
        await AddressableMgr.LoadAllByLabel<AuthorityData>(LABEL, (datas) =>
        {
            authorityDatas = datas;
            foreach (AuthorityData authorityData in authorityDatas)
            {
                authorityDataDic[authorityData.key] = authorityData;
            }
        });
    }

    public AuthorityData GetAuthorityData(string key)
    {
        return authorityDataDic.TryGetValue(key, out AuthorityData data) ? data : null;
    }

    // 묶음(1~6)의 권능 3개
    public List<AuthorityData> GetTierAuthorities(int tier)
    {
        return authorityDatas.Where(r => r.tier == tier).OrderBy(r => r.key).ToList();
    }

    // 로비에 장착된 슬라임들의 유형 태그 (권능 해당 종 수 계산용)
    public static List<MinionTag> GetEquippedMinionTags()
    {
        return UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions
            .Where(s => s != null && s.MinionData != null)
            .Select(s => s.MinionData.tags)
            .ToList();
    }
}
