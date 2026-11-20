using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class UserMinionManager : UserBaseManager
{
    public const string UserDataFileName = "UserMinionData";

    [field: SerializeField]
    public UserMinionData userMinionData
    {
        get; private set;
    }

    public override void LoadData()
    {
        userMinionData = SaveManager.LoadData<UserMinionData>(UserDataFileName);
        if (userMinionData == null)
        {
            userMinionData = new UserMinionData();
            for (int i = 0; i < GameSetting.INIT_MINION_KEYS.Length; i++)
            {
                UserMinion userMinion = GetUserMinion(GameSetting.INIT_MINION_KEYS[i]);
                userMinion.equipedIdx = i;
                userMinion.own = true;
            }
        }

        for (int i = 0; i < GameSetting.MINION_SLOT_COUNT; i++)
        {
            if (userMinionData.equiptedMinions[i] == null || string.IsNullOrEmpty(userMinionData.equiptedMinions[i].key) || !userMinionData.equiptedMinions[i].own)
            {
                userMinionData.equiptedMinions[i] = GetUserMinion(GameSetting.INIT_MINION_KEYS[i]);
                userMinionData.equiptedMinions[i].own = true;
                userMinionData.equiptedMinions[i].equipedIdx = i;
            }
            userMinionData.equiptedMinions[i] = GetEquiptUserMinion(i);
        }

        InitPresets();
        // (2026-11-20) 게임에서 빠진 미니언이 장착돼 있으면(이전 저장) 다른 보유 미니언으로 바꿈
        if (GetEquippedKeys().Any(k => !GameSetting.IsUsedMinion(k)))
            LoadPreset(userMinionData.selectedPresetIdx);
        RequestSave();
    }

    #region 슬라임 조합 프리셋
    // 장착 조합을 PRESET_COUNT개 저장. 선택된 프리셋은 장착이 바뀔 때마다 자동 저장된다.
    public const int PRESET_COUNT = 5;

    public int SelectedPresetIdx => userMinionData.selectedPresetIdx;

    void InitPresets()
    {
        if (userMinionData.presets == null || userMinionData.presets.Length != PRESET_COUNT)
        {
            MinionPreset[] old = userMinionData.presets;
            userMinionData.presets = new MinionPreset[PRESET_COUNT];
            for (int i = 0; i < PRESET_COUNT; i++)
            {
                if (old != null && i < old.Length)
                    userMinionData.presets[i] = old[i];
            }
        }

        string[] currentKeys = GetEquippedKeys();
        for (int i = 0; i < PRESET_COUNT; i++)
        {
            // 비어 있는 프리셋은 현재 장착 조합으로 채움
            if (userMinionData.presets[i] == null || userMinionData.presets[i].keys == null
                || userMinionData.presets[i].keys.Length != GameSetting.MINION_SLOT_COUNT)
                userMinionData.presets[i] = new MinionPreset { keys = (string[])currentKeys.Clone() };
        }

        userMinionData.selectedPresetIdx = Mathf.Clamp(userMinionData.selectedPresetIdx, 0, PRESET_COUNT - 1);
        // 현재 장착 = 선택된 프리셋
        SaveCurrentPreset();
    }

    string[] GetEquippedKeys()
    {
        string[] keys = new string[GameSetting.MINION_SLOT_COUNT];
        for (int i = 0; i < keys.Length; i++)
            keys[i] = userMinionData.equiptedMinions[i]?.key;
        return keys;
    }

    void SaveCurrentPreset()
    {
        if (userMinionData.presets == null)
            return;
        userMinionData.presets[userMinionData.selectedPresetIdx].keys = GetEquippedKeys();
    }

    public string[] GetPresetKeys(int presetIdx)
    {
        return userMinionData.presets[presetIdx].keys;
    }

    // 프리셋을 선택해 그 조합으로 장착. 보유하지 않았거나 중복된 key는 현재 장착/보유 슬라임으로 대체.
    public void LoadPreset(int presetIdx)
    {
        if (presetIdx < 0 || presetIdx >= PRESET_COUNT)
            return;

        // 게임에서 빠진 미니언(수확·팔랑이 등)은 보유해도 장착 안 함
        string[] ownedKeys = userMinionData.userMinions.Where(e => e.own && GameSetting.IsUsedMinion(e.key)).Select(e => e.key).ToArray();
        string[] keys = ResolvePresetKeys(userMinionData.presets[presetIdx].keys, GetEquippedKeys(), ownedKeys);

        foreach (UserMinion userMinion in userMinionData.userMinions)
            userMinion.equipedIdx = -1;
        for (int i = 0; i < keys.Length; i++)
            GetUserMinion(keys[i]).equipedIdx = i;

        userMinionData.selectedPresetIdx = presetIdx;
        RefreshEquippedMinions();
        SaveCurrentPreset();
        RequestSave();
    }

    /// <summary>
    /// 프리셋 key를 실제 장착할 key로 정리 (씬 없이 계산, 테스트용 static).
    /// 칸마다 프리셋 key가 보유 + 미중복이면 사용, 아니면 그 칸의 현재 장착 key → 남은 보유 key 순으로 대체.
    /// </summary>
    public static string[] ResolvePresetKeys(string[] presetKeys, string[] currentKeys, IEnumerable<string> ownedKeys)
    {
        HashSet<string> owned = new HashSet<string>(ownedKeys);
        int count = currentKeys.Length;
        string[] result = new string[count];
        HashSet<string> used = new HashSet<string>();

        bool Usable(string key) => !string.IsNullOrEmpty(key) && owned.Contains(key) && !used.Contains(key);

        // 1) 프리셋 key 중 쓸 수 있는 것 먼저 확정
        for (int i = 0; i < count; i++)
        {
            string key = presetKeys != null && i < presetKeys.Length ? presetKeys[i] : null;
            if (Usable(key))
            {
                result[i] = key;
                used.Add(key);
            }
        }

        // 2) 빈 칸은 현재 장착 key → 남은 보유 key
        for (int i = 0; i < count; i++)
        {
            if (result[i] != null)
                continue;
            string key = Usable(currentKeys[i]) ? currentKeys[i] : currentKeys.FirstOrDefault(Usable);
            if (key == null)
                key = owned.FirstOrDefault(Usable);
            result[i] = key;
            if (key != null)
                used.Add(key);
        }
        return result;
    }

    void RefreshEquippedMinions()
    {
        for (int i = 0; i < GameSetting.MINION_SLOT_COUNT; i++)
            userMinionData.equiptedMinions[i] = GetEquiptUserMinion(i);
    }
    #endregion


    public UserMinion GetEquiptUserMinion(int idx)
    {
        return userMinionData.userMinions.Where(e => e.equipedIdx == idx).FirstOrDefault();
    }

    public UserMinion GetUserMinion(string key)
    {
        UserMinion userMinion = userMinionData.userMinions.Where(e => e.key == key).FirstOrDefault();
        if (userMinion == null)
        {
            userMinion = new UserMinion();
            userMinion.key = key;
            userMinionData.userMinions.Add(userMinion);
            RequestSave();
        }

        return userMinion;
    }

    public UserMinion AddUserMinion(string key)
    {
        UserMinion userMinion = GetUserMinion(key);
        if (!userMinion.own)
        {
            userMinion.own = true;
            RequestSave();
        }
        return userMinion;
    }

    // 유저 레벨 lv까지 지급되는 미니언 key (GameSetting.MINION_LEVEL_UNLOCKS, 씬 없이 계산)
    public static string[] GetLevelUnlockKeys(int lv) =>
        GameSetting.MINION_LEVEL_UNLOCKS.Where(e => lv >= e.lv).Select(e => e.key).ToArray();

    // 레벨 지급 미니언 중 아직 없는 것을 보유 처리. 새로 받은 key 반환 (로드 후·레벨업 시 호출)
    public List<string> GrantLevelUnlockMinions(int lv)
    {
        List<string> granted = new List<string>();
        foreach (string key in GetLevelUnlockKeys(lv))
        {
            if (GetUserMinion(key).own)
                continue;
            AddUserMinion(key);
            granted.Add(key);
        }
        return granted;
    }

    public UserMinion EquiptUserMinion(UserMinion userMinion, int idx)
    {
        if (userMinion == null || userMinion.equipedIdx == idx)
            return userMinion;

        // 이미 다른 칸에 장착된 슬라임이면 두 칸을 맞바꿈 (5칸이 항상 차 있도록)
        int prevIdx = userMinion.equipedIdx;
        UserMinion prev = GetEquiptUserMinion(idx);
        if (prev != null)
            prev.equipedIdx = prevIdx >= 0 ? prevIdx : -1;

        userMinion.equipedIdx = idx;

        RefreshEquippedMinions();
        SaveCurrentPreset(); // 선택된 프리셋에 자동 저장
        RequestSave();
        return userMinion;
    }

    public UserMinion ReleaseUserMinion(int idx)
    {
        UserMinion userMinion = GetEquiptUserMinion(idx);
        if (userMinion == null)
            return null;
        userMinion.equipedIdx = -1;

        RefreshEquippedMinions();
        RequestSave();
        return userMinion;
    }
    public UserMinion ReleaseUserMinion(string key)
    {
        UserMinion userMinion = GetUserMinion(key);
        userMinion.equipedIdx = -1;

        RequestSave();
        return userMinion;
    }


    public override void SaveData()
    {
        SaveManager.SaveData(UserDataFileName, userMinionData);
    }

    public void AddExp(string key, int exp)
    {
        GetUserMinion(key).exp += exp;
        RequestSave();
    }

    // 레벨업 가능: 최대 레벨이 아니고 모인 경험치 >= 이번 레벨 필요량
    public bool CanLevelUp(string key)
    {
        UserMinion userMinion = GetUserMinion(key);
        EnhanceExpInfo info = MinionManager.Instance.GetEnhanceExpInfo();
        int lv = userMinion.EnhanceLevel;
        return info != null && !info.IsMaxLevel(lv) && userMinion.exp >= info.exps[lv];
    }

    // 이번 레벨 필요 경험치를 소모하고 레벨 +1 (조건은 CanLevelUp)
    public bool LevelUp(string key)
    {
        if (!CanLevelUp(key))
            return false;
        UserMinion userMinion = GetUserMinion(key);
        int lv = userMinion.EnhanceLevel;
        userMinion.exp -= MinionManager.Instance.GetEnhanceExpInfo().exps[lv];
        userMinion.enhanceLevel = lv + 1;
        GameEventBus.Publish(new LevelUpMinionEvent(key));
        AchievementManager.Report(AchievementType.LevelUpMinion);
        RequestSave();
        return true;
    }
}



[System.Serializable]
public class UserMinionData
{
    public UserMinion[] equiptedMinions = new UserMinion[GameSetting.MINION_SLOT_COUNT];
    public List<UserMinion> userMinions = new List<UserMinion>(); //바ㅇ
    public int selectedPresetIdx = 0;          // 선택된 조합 프리셋 (기본 1번 = 0)
    public MinionPreset[] presets;              // 조합 프리셋 (UserMinionManager.PRESET_COUNT개)
}

[System.Serializable]
public class MinionPreset
{
    public string[] keys; // 칸별 슬라임 key (MINION_SLOT_COUNT개)
}

[System.Serializable]
public class UserMinion
{
    public string key;
    public int equipedIdx = -1;
    public bool Equiping => equipedIdx >= 0;
    public bool own;
    public int enhanceLevel = 0; // 강화 레벨 (0부터, 미니언 등급 없음 → 시작 레벨 보정 없음)
    public int exp; // 지금 모인 경험치. 레벨업하면 그 레벨 필요량만큼 소모
    public MinionData MinionData => MinionManager.Instance.GetMinionData(key);

    public int EnhanceLevel => enhanceLevel;
}

[System.Serializable]
public class LevelUpMinionEvent
{
    public string key;
    public LevelUpMinionEvent(string key)
    {
        this.key = key;
    }
}