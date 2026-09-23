using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class UserDataManager : MonoSingleton<UserDataManager>
{
    public const string UserDataFileName = "UserData";
    public static string STAGE_KEY = GameSetting.FIRST_STAGE_KEY;// "Gateway1";
    
    public static DifficultyType difficultyType = DifficultyType.Normal;

    [field: SerializeField]
    public UserData userData
    {
        get; private set;
    }

    public UserStageManager userStageManager;
    public UserEquipmentManager userEquipmentManager;
    public UserSlimeManager userSlimeManager;
    public UserPurchaseManager userPurchaseManager;
    public UserMyInfoManager userMyInfoManager;

    private readonly List<UserBaseManager> dirtySaveManagers = new List<UserBaseManager>();

    public void RegisterDirtySave(UserBaseManager manager)
    {
        dirtySaveManagers.Add(manager);
    }

    void LateUpdate()
    {
        if (dirtySaveManagers.Count == 0)
            return;

        for (int i = 0; i < dirtySaveManagers.Count; i++)
        {
            dirtySaveManagers[i].FlushSave();
        }
        dirtySaveManagers.Clear();
    }

    void Awake()
    {
        Application.targetFrameRate = 60;

        userData = SaveManager.LoadData<UserData>(UserDataFileName);
        if (userData == null)
        {
            userData = new UserData();
            userData.energe = GameSetting.MAX_ENERGE;
        }
        Debug.Log("UserManager Awake()");
        
        userStageManager = new UserStageManager();
        userEquipmentManager = new UserEquipmentManager();
        userPurchaseManager = new UserPurchaseManager();
        userSlimeManager = new UserSlimeManager();
        userMyInfoManager= new UserMyInfoManager();

        // userBulletManager.LoadData();
        userStageManager.LoadData();
        userEquipmentManager.LoadData();
        userPurchaseManager.LoadData();
        userSlimeManager.LoadData();
        userMyInfoManager.LoadData();

    }
    void Save()
    {
        SaveManager.SaveData(UserDataFileName, userData);
    }

    public void AddGold(int count)
    {
        userData.gold += count;
        Save();
    }
    public void AddDia(int count)
    {
        userData.dia += count;
        Save();
    }

    public void AddEnerge(int count)
    {
        userData.energe += count;
        Save();
    }

    public void AddDrawTicket(int drawTicket)
    {
        userData.drawTicket += drawTicket;
        Save();
    }


}

[System.Serializable]
public class UserData
{
    public int gold; //던전 수정 조작 느낌으로
    public int dia; //던전 수정 조작 느낌으로
    public int energe;
    public int drawTicket;

    public int GetCurrency(CurrencyType currencyType)
    {
        switch (currencyType)
        {
            case CurrencyType.Gold:
                return gold;
            case CurrencyType.Dia:
                return dia;
            case CurrencyType.Energe:
                return energe;
            case CurrencyType.DrawTicket:
                return drawTicket;
        }
        return 0;
    }
    public string characterName = "Lucky";
    public CharacterName CharacterName => Enum.Parse<CharacterName>(characterName);
}
public enum CurrencyType
{
    Gold, Dia,Energe,DrawTicket
}