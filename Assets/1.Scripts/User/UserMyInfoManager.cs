using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class UserMyInfoManager : UserBaseManager
{
    public const string UserDataFileName = "UserMyInfoData";

    [field: SerializeField]
    public UserMyInfoData userMyInfoData
    {
        get; private set;
    }

    public override void LoadData()
    {
        userMyInfoData = SaveManager.LoadData<UserMyInfoData>(UserDataFileName);
        if(userMyInfoData == null)
        {
            userMyInfoData= new UserMyInfoData();
            userMyInfoData.nickname = System.Guid.NewGuid().ToString();
        }

        RequestSave();
    }
    public override void SaveData()
    {
        SaveManager.SaveData(UserDataFileName, userMyInfoData);
    }


    public void AddExp(int exp)
    {
        userMyInfoData.exp += exp;
        RequestSave();
        GameEventBus.Publish(new ChangedUserExpEvent());
    }

    public int GetMaxExp(int lv = -1)
    {
        if (lv < 0)
        {
            lv = userMyInfoData.lv;
        }

        return 100 + (lv * 25);
    }

    public bool CheckLevelUp()
    {
        int remainExp = userMyInfoData.exp - GetMaxExp(userMyInfoData.lv);
        if (remainExp >= 0)
        {
            return true;
        }
        return false;
    }

    public bool LevelUp()
    {
        int remainExp = userMyInfoData.exp - GetMaxExp(userMyInfoData.lv);
        if (remainExp >= 0)
        {   
            userMyInfoData.exp = remainExp;
            userMyInfoData.lv++;
            RequestSave();
            GameEventBus.Publish(new ChangedUserExpEvent());
            return true;
        }
        return false;
    }
}



// 유저 경험치/레벨 변경 시 발행 (MyInfoPanel 갱신)
public class ChangedUserExpEvent { }

[System.Serializable]
public class UserMyInfoData
{
    public int exp;
    public int lv;
    public string nickname;
    public int nickChangeCount;
}
