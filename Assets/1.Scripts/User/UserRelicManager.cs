using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// 유물 선택 상태. 묶음(1~6)마다 고른 유물 key 하나, 해금된 묶음의 선택은 전부 적용 (CLAUDE.md "유물")
// 해금 = 유저 레벨 ≥ RelicData.TIER_UNLOCK_LEVELS[tier - 1]. 로비에서 언제든 무료로 바꿀 수 있다
[System.Serializable]
public class UserRelicManager : UserBaseManager
{
    public const string UserDataFileName = "UserRelicData";

    [field: SerializeField]
    public UserRelicData userRelicData
    {
        get; private set;
    }

    public override void LoadData()
    {
        userRelicData = SaveManager.LoadData<UserRelicData>(UserDataFileName);
        if (userRelicData == null)
            userRelicData = new UserRelicData();
        userRelicData.Normalize();
        RequestSave();
    }

    public override void SaveData()
    {
        SaveManager.SaveData(UserDataFileName, userRelicData);
    }

    static int UserLv => UserDataManager.Instance.userMyInfoManager.userMyInfoData.lv;

    public bool IsTierUnlocked(int tier) => IsTierUnlocked(tier, UserLv);
    public static bool IsTierUnlocked(int tier, int userLv) => tier >= 1 && tier <= RelicData.TierCount && userLv >= RelicData.GetUnlockLevel(tier);

    // 묶음에서 고른 유물 key (없으면 null)
    public string GetSelectedKey(int tier)
    {
        string key = userRelicData.selectedKeys[tier - 1];
        return string.IsNullOrEmpty(key) ? null : key;
    }

    public bool IsSelected(RelicData relic) => relic != null && GetSelectedKey(relic.tier) == relic.key;

    // 선택 가능 여부 (씬·싱글톤 없이 판정 → 테스트)
    public static RelicSelectResult CheckSelect(int userLv, RelicData relic)
    {
        if (relic == null)
            return RelicSelectResult.NoData;
        if (!IsTierUnlocked(relic.tier, userLv))
            return RelicSelectResult.Locked;
        return RelicSelectResult.Success;
    }

    // 같은 묶음의 이전 선택은 바뀜
    public RelicSelectResult Select(RelicData relic)
    {
        RelicSelectResult result = CheckSelect(UserLv, relic);
        if (result != RelicSelectResult.Success)
            return result;
        userRelicData.selectedKeys[relic.tier - 1] = relic.key;
        RequestSave();
        return result;
    }

    // 지금 적용되는 유물 key (해금된 묶음의 선택만)
    public List<string> GetActiveRelicKeys()
    {
        var keys = new List<string>();
        for (int tier = 1; tier <= RelicData.TierCount; tier++)
        {
            string key = GetSelectedKey(tier);
            if (key != null && IsTierUnlocked(tier))
                keys.Add(key);
        }
        return keys;
    }

    // 해금됐는데 아직 고르지 않은 묶음 (로비에서 선택 안내)
    public List<int> GetUnselectedUnlockedTiers()
    {
        var tiers = new List<int>();
        for (int tier = 1; tier <= RelicData.TierCount; tier++)
            if (IsTierUnlocked(tier) && GetSelectedKey(tier) == null)
                tiers.Add(tier);
        return tiers;
    }
}

public enum RelicSelectResult
{
    Success,
    Locked,   // 유저 레벨이 묶음 해금 레벨보다 낮음
    NoData,
}

[System.Serializable]
public class UserRelicData
{
    // index = 묶음 - 1. 비어 있으면 아직 안 고름
    public string[] selectedKeys = new string[RelicData.TIER_UNLOCK_LEVELS.Length];

    // 저장 파일의 배열 길이가 묶음 수와 다르면 맞춤 (묶음 추가 등)
    public void Normalize()
    {
        int count = RelicData.TIER_UNLOCK_LEVELS.Length;
        if (selectedKeys == null)
            selectedKeys = new string[count];
        else if (selectedKeys.Length != count)
            System.Array.Resize(ref selectedKeys, count);
    }
}
