using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class UserSlimeManager : UserBaseManager
{
    public const string UserDataFileName = "UserSlimeData";

    [field: SerializeField]
    public UserSlimeData userSlimeData
    {
        get; private set;
    }

    public override void LoadData()
    {
        userSlimeData = SaveManager.LoadData<UserSlimeData>(UserDataFileName);
        if (userSlimeData == null)
        {
#if UNITY_EDITOR
            GameSetting.INIT_SLIME_KEYS = new string[] { "Flame", "Ice", "Random", "Orbit", "Lightning" };
#endif
            userSlimeData = new UserSlimeData();
            for (int i = 0; i < GameSetting.INIT_SLIME_KEYS.Length; i++)
            {
                UserSlime userSlime = GetUserSlime(GameSetting.INIT_SLIME_KEYS[i]);
                userSlime.equipedIdx = i;
                userSlime.own = true;
            }
        }

        for (int i = 0; i < GameSetting.SLIME_SLOT_COUNT; i++)
        {
            if (userSlimeData.equiptedSlimes[i] == null || string.IsNullOrEmpty(userSlimeData.equiptedSlimes[i].key) || !userSlimeData.equiptedSlimes[i].own)
            {
                userSlimeData.equiptedSlimes[i] = GetUserSlime(GameSetting.INIT_SLIME_KEYS[i]);
                userSlimeData.equiptedSlimes[i].own = true;
                userSlimeData.equiptedSlimes[i].equipedIdx = i;
            }
            userSlimeData.equiptedSlimes[i] = GetEquiptUserSlime(i);
        }

        InitPresets();
        RequestSave();
    }

    #region 슬라임 조합 프리셋
    // 장착 조합을 PRESET_COUNT개 저장. 선택된 프리셋은 장착이 바뀔 때마다 자동 저장된다.
    public const int PRESET_COUNT = 5;

    public int SelectedPresetIdx => userSlimeData.selectedPresetIdx;

    void InitPresets()
    {
        if (userSlimeData.presets == null || userSlimeData.presets.Length != PRESET_COUNT)
        {
            SlimePreset[] old = userSlimeData.presets;
            userSlimeData.presets = new SlimePreset[PRESET_COUNT];
            for (int i = 0; i < PRESET_COUNT; i++)
            {
                if (old != null && i < old.Length)
                    userSlimeData.presets[i] = old[i];
            }
        }

        string[] currentKeys = GetEquippedKeys();
        for (int i = 0; i < PRESET_COUNT; i++)
        {
            // 비어 있는 프리셋은 현재 장착 조합으로 채움
            if (userSlimeData.presets[i] == null || userSlimeData.presets[i].keys == null
                || userSlimeData.presets[i].keys.Length != GameSetting.SLIME_SLOT_COUNT)
                userSlimeData.presets[i] = new SlimePreset { keys = (string[])currentKeys.Clone() };
        }

        userSlimeData.selectedPresetIdx = Mathf.Clamp(userSlimeData.selectedPresetIdx, 0, PRESET_COUNT - 1);
        // 현재 장착 = 선택된 프리셋
        SaveCurrentPreset();
    }

    string[] GetEquippedKeys()
    {
        string[] keys = new string[GameSetting.SLIME_SLOT_COUNT];
        for (int i = 0; i < keys.Length; i++)
            keys[i] = userSlimeData.equiptedSlimes[i]?.key;
        return keys;
    }

    void SaveCurrentPreset()
    {
        if (userSlimeData.presets == null)
            return;
        userSlimeData.presets[userSlimeData.selectedPresetIdx].keys = GetEquippedKeys();
    }

    public string[] GetPresetKeys(int presetIdx)
    {
        return userSlimeData.presets[presetIdx].keys;
    }

    // 프리셋을 선택해 그 조합으로 장착. 보유하지 않았거나 중복된 key는 현재 장착/보유 슬라임으로 대체.
    public void LoadPreset(int presetIdx)
    {
        if (presetIdx < 0 || presetIdx >= PRESET_COUNT)
            return;

        string[] ownedKeys = userSlimeData.userSlimes.Where(e => e.own).Select(e => e.key).ToArray();
        string[] keys = ResolvePresetKeys(userSlimeData.presets[presetIdx].keys, GetEquippedKeys(), ownedKeys);

        foreach (UserSlime userSlime in userSlimeData.userSlimes)
            userSlime.equipedIdx = -1;
        for (int i = 0; i < keys.Length; i++)
            GetUserSlime(keys[i]).equipedIdx = i;

        userSlimeData.selectedPresetIdx = presetIdx;
        RefreshEquippedSlimes();
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

    void RefreshEquippedSlimes()
    {
        for (int i = 0; i < GameSetting.SLIME_SLOT_COUNT; i++)
            userSlimeData.equiptedSlimes[i] = GetEquiptUserSlime(i);
    }
    #endregion


    public UserSlime GetEquiptUserSlime(int idx)
    {
        return userSlimeData.userSlimes.Where(e => e.equipedIdx == idx).FirstOrDefault();
    }

    public UserSlime GetUserSlime(string key)
    {
        UserSlime userSlime = userSlimeData.userSlimes.Where(e => e.key == key).FirstOrDefault();
        if (userSlime == null)
        {
            userSlime = new UserSlime();
            userSlime.key = key;
            userSlimeData.userSlimes.Add(userSlime);
            RequestSave();
        }

        return userSlime;
    }

    public UserSlime AddUserSlime(string key)
    {
        UserSlime userSlime = GetUserSlime(key);
        if (!userSlime.own)
        {
            userSlime.own = true;
            RequestSave();
        }
        return userSlime;
    }

    public UserSlime EquiptUserSlime(UserSlime userSlime, int idx)
    {
        if (userSlime == null || userSlime.equipedIdx == idx)
            return userSlime;

        // 이미 다른 칸에 장착된 슬라임이면 두 칸을 맞바꿈 (5칸이 항상 차 있도록)
        int prevIdx = userSlime.equipedIdx;
        UserSlime prev = GetEquiptUserSlime(idx);
        if (prev != null)
            prev.equipedIdx = prevIdx >= 0 ? prevIdx : -1;

        userSlime.equipedIdx = idx;

        RefreshEquippedSlimes();
        SaveCurrentPreset(); // 선택된 프리셋에 자동 저장
        RequestSave();
        return userSlime;
    }

    public UserSlime ReleaseUserSlime(int idx)
    {
        UserSlime userSlime = GetEquiptUserSlime(idx);
        if (userSlime == null)
            return null;
        userSlime.equipedIdx = -1;

        RefreshEquippedSlimes();
        RequestSave();
        return userSlime;
    }
    public UserSlime ReleaseUserSlime(string key)
    {
        UserSlime userSlime = GetUserSlime(key);
        userSlime.equipedIdx = -1;

        RequestSave();
        return userSlime;
    }


    public override void SaveData()
    {
        SaveManager.SaveData(UserDataFileName, userSlimeData);
    }

    public void AddExp(string key, int exp)
    {
        GetUserSlime(key).exp += exp;
        RequestSave();
    }

    public void LevelUp(string key)
    {
        GetUserSlime(key).enhanceLevel++;
        GameEventBus.Publish(new LevelUpSlimeEvent(key));
        RequestSave();
    }
}



[System.Serializable]
public class UserSlimeData
{
    public UserSlime[] equiptedSlimes = new UserSlime[GameSetting.SLIME_SLOT_COUNT];
    public List<UserSlime> userSlimes = new List<UserSlime>(); //바ㅇ
    public int selectedPresetIdx = 0;          // 선택된 조합 프리셋 (기본 1번 = 0)
    public SlimePreset[] presets;              // 조합 프리셋 (UserSlimeManager.PRESET_COUNT개)
}

[System.Serializable]
public class SlimePreset
{
    public string[] keys; // 칸별 슬라임 key (SLIME_SLOT_COUNT개)
}

[System.Serializable]
public class UserSlime
{
    public string key;
    public int equipedIdx = -1;
    public bool Equiping => equipedIdx >= 0;
    public bool own;
    public int enhanceLevel = 0;
    public int exp;
    public SlimeData SlimeData => SlimeManager.Instance.GetSlimeData(key);
}

[System.Serializable]
public class LevelUpSlimeEvent
{
    public string key;
    public LevelUpSlimeEvent(string key)
    {
        this.key = key;
    }
}