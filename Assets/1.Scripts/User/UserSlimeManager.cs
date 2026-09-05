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
                // Debug.Log($"조건 걸림 i {i} {GameSetting.INIT_BULLE_KEYS[i]}");
                userSlimeData.equiptedSlimes[i] = GetUserSlime(GameSetting.INIT_SLIME_KEYS[i]);
                userSlimeData.equiptedSlimes[i].own = true;
                userSlimeData.equiptedSlimes[i].equipedIdx = i;
            }
            userSlimeData.equiptedSlimes[i] = GetEquiptUserSlime(i);
        }

        SaveData();
    }


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
            // Debug.Log($"GetUserSlime key {key} 새로 만들자 저장하자");
            SaveData();
        }
        else
        {
            // Debug.Log($"GetUserSlime key {key} 이미 있음");
        }

        return userSlime;
    }

    public UserSlime AddUserSlime(string key)
    {
        UserSlime userSlime = GetUserSlime(key);
        if (!userSlime.own)
        {
            userSlime.own = true;
            SaveData();
        }
        return userSlime;
    }

    public UserSlime EquiptUserSlime(UserSlime userSlime, int idx)
    {
        ReleaseUserSlime(idx);

        userSlime.equipedIdx = idx;

        SaveData();
        return userSlime;
    }

    public UserSlime ReleaseUserSlime(int idx)
    {
        UserSlime userSlime = GetEquiptUserSlime(idx);
        userSlime.equipedIdx = -1;

        SaveData();
        return userSlime;
    }
    public UserSlime ReleaseUserSlime(string key)
    {
        UserSlime userSlime = GetUserSlime(key);
        userSlime.equipedIdx = -1;

        SaveData();
        return userSlime;
    }


    public override void SaveData()
    {
        SaveManager.SaveData(UserDataFileName, userSlimeData);
    }

    public void AddExp(string key, int exp)
    {
        GetUserSlime(key).exp += exp;
        SaveData();
    }
}



[System.Serializable]
public class UserSlimeData
{
    public UserSlime[] equiptedSlimes = new UserSlime[GameSetting.SLIME_SLOT_COUNT];
    public List<UserSlime> userSlimes = new List<UserSlime>(); //바ㅇ
}

[System.Serializable]
public class UserSlime
{
    public string key;
    public int equipedIdx = -1;
    public bool Equiping => equipedIdx >= 0;
    public bool own;
    public int enhanceLevel;
    public int EnhanceLevel(bool includeBaseLv = true)
    {
        if (includeBaseLv)
        {
            if (SlimeData == null)
            {
                Debug.Log($"if(SlimeData == null) key : {key}");
            }

            return enhanceLevel + SlimeManager.Instance.GetEnhanceGradeInfo(SlimeData.grade).baseEnhance;
        }

        else
            return enhanceLevel;
    }
    public int exp;
    public SlimeData SlimeData => SlimeManager.Instance.GetSlimeData(key);
}