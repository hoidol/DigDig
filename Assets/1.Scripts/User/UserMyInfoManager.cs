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

    // 유저 최대 레벨. 업데이트로 올릴 수 있음 (최대 레벨 동안 경험치는 0으로 유지 → 올리면 0부터 다시 쌓임)
    public const int MAX_LEVEL = 30;

    public static bool IsMaxLevel(int lv) => lv >= MAX_LEVEL;

    // 받을 경험치. 최대 레벨이면 0
    public static int GetGainExp(int lv, int exp) => IsMaxLevel(lv) ? 0 : Mathf.Max(0, exp);

    public bool IsMaxLv => IsMaxLevel(userMyInfoData.lv);

    // 인게임 결과로 받았지만 로비 경험치 바에 아직 안 보여준 경험치 (로비 진입 시 바가 차오르는 연출에 쓰고 0으로. 저장 안 함)
    public static int unshownGainExp;

    // 마지막 LevelUp에서 새로 받은 미니언 key (저장 안 함)
    [System.NonSerialized] public List<string> lastUnlockedMinionKeys = new List<string>();

    public override void LoadData()
    {
        userMyInfoData = SaveManager.LoadData<UserMyInfoData>(UserDataFileName);
        if(userMyInfoData == null)
        {
            userMyInfoData= new UserMyInfoData();
            userMyInfoData.nickname = System.Guid.NewGuid().ToString();
        }

        if (IsMaxLv)
            userMyInfoData.exp = 0;

        RequestSave();
    }
    public override void SaveData()
    {
        SaveManager.SaveData(UserDataFileName, userMyInfoData);
    }


    public void AddExp(int exp)
    {
        exp = GetGainExp(userMyInfoData.lv, exp);
        if (exp <= 0)
            return;

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
        if (IsMaxLv)
            return false;

        int remainExp = userMyInfoData.exp - GetMaxExp(userMyInfoData.lv);
        if (remainExp >= 0)
        {
            return true;
        }
        return false;
    }

    public bool LevelUp()
    {
        if (IsMaxLv)
            return false;

        int remainExp = userMyInfoData.exp - GetMaxExp(userMyInfoData.lv);
        if (remainExp >= 0)
        {
            userMyInfoData.lv++;
            // 최대 레벨에 도달하면 남은 경험치는 버림
            userMyInfoData.exp = IsMaxLv ? 0 : remainExp;
            // 이 레벨에 구매할 수 있게 된 미니언 (로비 레벨업 창 뒤에 토스트, 2026-11-24 자동 지급 → 구매)
            lastUnlockedMinionKeys = new List<string>(UserMinionManager.GetLevelUnlockKeysAt(userMyInfoData.lv));
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
