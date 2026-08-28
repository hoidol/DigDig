using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class UserMiniMeManager : UserBaseManager
{
    public const string UserDataFileName = "UserMiniMeData";

    [field: SerializeField]
    public UserMiniMeData userMiniMeData
    {
        get; private set;
    }

    public override void LoadData()
    {
        userMiniMeData = SaveManager.LoadData<UserMiniMeData>(UserDataFileName);
        if (userMiniMeData == null)
        {
            userMiniMeData = new UserMiniMeData();
            for (int i = 0; i < GameSetting.INIT_MINIME_KEYS.Length; i++)
            {
                UserMiniMe userMiniMe = GetUserMiniMe(GameSetting.INIT_MINIME_KEYS[i]);
                userMiniMe.equipedIdx = i;
                userMiniMe.own = true;
            }
        }

        for (int i = 0; i < GameSetting.MINIME_SLOT_COUNT; i++)
        {
            if (userMiniMeData.equiptedMiniMes[i] == null || string.IsNullOrEmpty(userMiniMeData.equiptedMiniMes[i].key) || !userMiniMeData.equiptedMiniMes[i].own)
            {
                // Debug.Log($"조건 걸림 i {i} {GameSetting.INIT_BULLE_KEYS[i]}");
                userMiniMeData.equiptedMiniMes[i] = GetUserMiniMe(GameSetting.INIT_MINIME_KEYS[i]);
                userMiniMeData.equiptedMiniMes[i].own = true;
                userMiniMeData.equiptedMiniMes[i].equipedIdx = i;
            }
            userMiniMeData.equiptedMiniMes[i] = GetEquiptUserMiniMe(i);
        }

        SaveData();
    }


    public UserMiniMe GetEquiptUserMiniMe(int idx)
    {
        return userMiniMeData.userMiniMes.Where(e => e.equipedIdx == idx).FirstOrDefault();
    }

    public UserMiniMe GetUserMiniMe(string key)
    {
        UserMiniMe userMiniMe = userMiniMeData.userMiniMes.Where(e => e.key == key).FirstOrDefault();
        if (userMiniMe == null)
        {
            userMiniMe = new UserMiniMe();
            userMiniMe.key = key;
            userMiniMeData.userMiniMes.Add(userMiniMe);
            Debug.Log($"GetUserMiniMe key {key} 새로 만들자 저장하자");
            SaveData();
        }
        else
        {
            Debug.Log($"GetUserMiniMe key {key} 이미 있음");
        }

        return userMiniMe;
    }

    public UserMiniMe AddUserMiniMe(string key)
    {
        UserMiniMe userMiniMe = GetUserMiniMe(key);
        if (!userMiniMe.own)
        {
            userMiniMe.own = true;
            SaveData();
        }
        return userMiniMe;
    }

    public UserMiniMe EquiptUserMiniMe(UserMiniMe userMiniMe, int idx)
    {
        ReleaseUserMiniMe(idx);
        
        userMiniMe.equipedIdx = idx;

        SaveData();
        return userMiniMe;
    }

    public UserMiniMe ReleaseUserMiniMe(int idx)
    {
        UserMiniMe userMiniMe = GetEquiptUserMiniMe(idx);
        userMiniMe.equipedIdx = -1;

        SaveData();
        return userMiniMe;
    }
    public UserMiniMe ReleaseUserMiniMe(string key)
    {
        UserMiniMe userMiniMe = GetUserMiniMe(key);
        userMiniMe.equipedIdx = -1;

        SaveData();
        return userMiniMe;
    }


    public override void SaveData()
    {
        SaveManager.SaveData(UserDataFileName, userMiniMeData);
    }
}



[System.Serializable]
public class UserMiniMeData
{
    public UserMiniMe[] equiptedMiniMes = new UserMiniMe[GameSetting.MINIME_SLOT_COUNT];
    public List<UserMiniMe> userMiniMes = new List<UserMiniMe>(); //바ㅇ
}

[System.Serializable]
public class UserMiniMe
{
    public string key;
    public int equipedIdx = -1;
    public bool Equiping => equipedIdx >=0;
    public bool own;
    public int lv;
    public int exp;
    public MiniMeData MiniMeData => MiniMeManager.Instance.GetMiniMeData(key);
}