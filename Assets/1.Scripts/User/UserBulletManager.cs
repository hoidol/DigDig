using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UserBulletManager : UserBaseManager
{
    public const string UserDataFileName = "UserBulletData";
    
    [field: SerializeField]
    public UserBulletData userBulletData
    {
        get; private set;
    }

    public override void LoadData()
    {
        userBulletData = SaveManager.LoadData<UserBulletData>(UserDataFileName);
        if (userBulletData == null)
        {
            for (int i = 0; i < GameSetting.INIT_BULLE_KEYS.Length; i++)
            {
                UserBullet userBullet = GetUserBullet(GameSetting.INIT_BULLE_KEYS[i]);
                userBullet.equipedIdx = i;
            }
        }

        for(int i = 0; i < 5; i++)
        {
            userBulletData.equiptedBullets[i] = GetEquiptUserBullet(i);
        }
    }


    public UserBullet GetEquiptUserBullet(int idx)
    {
        return userBulletData.userBullets.Where(e=>e.equipedIdx == idx ).FirstOrDefault();
    }

    public UserBullet GetUserBullet(string key)
    {
        UserBullet userBullet = userBulletData.userBullets.Where(e=>e.key ==key).FirstOrDefault();
        if(userBullet == null)
        {
            userBullet = new UserBullet();
            userBullet.key = key;
            userBulletData.userBullets.Add(userBullet);
            SaveData();
        }

        return userBullet;
    }

    public UserBullet AddUserBullet(string key)
    {
        UserBullet userBullet = GetUserBullet(key);
        if (!userBullet.own)
        {
            userBullet.own = true;  
            SaveData();
        }
        return userBullet;
    }

    public UserBullet EquiptUserBullet(string key, int idx)
    {
        ReleaseUserBullet(idx);
        UserBullet userBullet = GetUserBullet(key);
        userBullet.equipedIdx = idx;
        
        SaveData();
        return userBullet;
    }

    public UserBullet ReleaseUserBullet(int idx)
    {
        UserBullet userBullet = GetEquiptUserBullet(idx);
        userBullet.equipedIdx= -1;
         
        SaveData();
        return userBullet;
    }
    public UserBullet ReleaseUserBullet(string key)
    {
        UserBullet userBullet = GetUserBullet(key);
        userBullet.equipedIdx= -1;
         
        SaveData();
        return userBullet;
    }


    public override void SaveData()
    {
            SaveManager.SaveData(UserDataFileName,userBulletData); 
    }
}



[System.Serializable]
public class UserBulletData
{
    public UserBullet[] equiptedBullets = new UserBullet[5];
    public List<UserBullet> userBullets = new List<UserBullet>(); //바ㅇ
}

[System.Serializable]
public class UserBullet
{
    public string key;
    public int equipedIdx = -1;
    public bool own;
    public int lv;
}