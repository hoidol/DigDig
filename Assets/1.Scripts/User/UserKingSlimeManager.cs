using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class UserKingSlimeManager : UserBaseManager
{
    public const string UserDataFileName = "UserKingSlimeData";

    [field: SerializeField]
    public UserKSData userKSData
    {
        get; private set;
    }

    public override void LoadData()
    {
        userKSData = SaveManager.LoadData<UserKSData>(UserDataFileName);
        if (userKSData == null)
        {
            userKSData = new UserKSData();
        }

        RequestSave();
    }

    // 데이터 없음 / 최대 Lv / 재화 부족이면 false
    public bool TryUpgrade(StatType statType)
    {
        KSUpgradeInfo info = KSUpgradeData.Instance.GetUpgradeInfo(statType);
        if (info == null)
            return false;

        int lv = GetUpgrade(statType).lv;
        if (info.IsMaxLv(lv))
            return false;

        if (!UserDataManager.Instance.TryUseCurrency(info.currencyType, info.GetPrice(lv)))
            return false;

        LvUpUpgrade(statType);
        return true;
    }

    public void LvUpUpgrade(StatType statType)
    {
        var upgrade = GetUpgrade(statType);
        upgrade.lv++;

        RequestSave();
        GameEventBus.Publish(new KSUpgradeEvent(statType, upgrade.lv));
    }

    // 업그레이드로 더해지는 스탯 (데이터 없으면 0)
    public float GetUpgradeValue(StatType statType)
    {
        KSUpgradeInfo info = KSUpgradeData.Instance.GetUpgradeInfo(statType);
        if (info == null)
            return 0f;
        return info.GetValue(GetUpgrade(statType).lv);
    }

    public UserKSUpgrade GetUpgrade(StatType statType)
    {
        var upgrade =  userKSData.userKSUpgrades.FirstOrDefault(u => u.statType == statType);
        if (upgrade == null)
        {
            upgrade = new UserKSUpgrade() { statType = statType, lv = 0 };
            userKSData.userKSUpgrades.Add(upgrade);
        }
        return upgrade;
    }


    public override void SaveData()
    {
        SaveManager.SaveData(UserDataFileName, userKSData);
    }
}



[System.Serializable]
public class UserKSData
{
    public List<UserKSUpgrade> userKSUpgrades = new List<UserKSUpgrade>(); //바ㅇ
}

[System.Serializable]
public class UserKSUpgrade
{
    public StatType statType;
    public int lv;
}

public class KSUpgradeEvent
{
    public StatType statType;
    public int lv;
    public KSUpgradeEvent(StatType statType, int lv) { this.statType = statType; this.lv = lv; }
}