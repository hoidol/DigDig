using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// 권능 선택 상태. 묶음(1~6)마다 고른 권능 key 하나, 해금된 묶음의 선택은 전부 적용 (CLAUDE.md "권능")
// 해금 = 유저 레벨 ≥ AuthorityData.TIER_UNLOCK_LEVELS[tier - 1]. 로비에서 언제든 무료로 바꿀 수 있다
[System.Serializable]
public class UserAuthorityManager : UserBaseManager
{
    public const string UserDataFileName = "UserAuthorityData";

    [field: SerializeField]
    public UserAuthorityData userAuthorityData
    {
        get; private set;
    }

    public override void LoadData()
    {
        userAuthorityData = SaveManager.LoadData<UserAuthorityData>(UserDataFileName);
        if (userAuthorityData == null)
            userAuthorityData = new UserAuthorityData();
        userAuthorityData.Normalize();
        RequestSave();
    }

    public override void SaveData()
    {
        SaveManager.SaveData(UserDataFileName, userAuthorityData);
    }

    static int UserLv => UserDataManager.Instance.userMyInfoManager.userMyInfoData.lv;

    public bool IsTierUnlocked(int tier) => IsTierUnlocked(tier, UserLv);
    public static bool IsTierUnlocked(int tier, int userLv) => tier >= 1 && tier <= AuthorityData.TierCount && userLv >= AuthorityData.GetUnlockLevel(tier);

    // 묶음에서 고른 권능 key (없으면 null)
    public string GetSelectedKey(int tier)
    {
        string key = userAuthorityData.selectedKeys[tier - 1];
        return string.IsNullOrEmpty(key) ? null : key;
    }

    public bool IsSelected(AuthorityData authority) => authority != null && GetSelectedKey(authority.tier) == authority.key;

    // 선택 가능 여부 (씬·싱글톤 없이 판정 → 테스트)
    public static AuthoritySelectResult CheckSelect(int userLv, AuthorityData authority)
    {
        if (authority == null)
            return AuthoritySelectResult.NoData;
        if (!IsTierUnlocked(authority.tier, userLv))
            return AuthoritySelectResult.Locked;
        return AuthoritySelectResult.Success;
    }

    // 같은 묶음의 이전 선택은 바뀜
    public AuthoritySelectResult Select(AuthorityData authority)
    {
        AuthoritySelectResult result = CheckSelect(UserLv, authority);
        if (result != AuthoritySelectResult.Success)
            return result;
        userAuthorityData.selectedKeys[authority.tier - 1] = authority.key;
        RequestSave();
        return result;
    }

    // 지금 적용되는 권능 key (해금된 묶음의 선택만)
    public List<string> GetActiveAuthorityKeys()
    {
        var keys = new List<string>();
        for (int tier = 1; tier <= AuthorityData.TierCount; tier++)
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
        for (int tier = 1; tier <= AuthorityData.TierCount; tier++)
            if (IsTierUnlocked(tier) && GetSelectedKey(tier) == null)
                tiers.Add(tier);
        return tiers;
    }
}

public enum AuthoritySelectResult
{
    Success,
    Locked,   // 유저 레벨이 묶음 해금 레벨보다 낮음
    NoData,
}

[System.Serializable]
public class UserAuthorityData
{
    // index = 묶음 - 1. 비어 있으면 아직 안 고름
    public string[] selectedKeys = new string[AuthorityData.TIER_UNLOCK_LEVELS.Length];

    // 저장 파일의 배열 길이가 묶음 수와 다르면 맞춤 (묶음 추가 등)
    public void Normalize()
    {
        int count = AuthorityData.TIER_UNLOCK_LEVELS.Length;
        if (selectedKeys == null)
            selectedKeys = new string[count];
        else if (selectedKeys.Length != count)
            System.Array.Resize(ref selectedKeys, count);
    }
}
