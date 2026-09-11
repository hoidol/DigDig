using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class UserManager : MonoSingleton<UserManager>
{
    public const string UserDataFileName = "UserData";
    public static string STAGE_KEY = GameSetting.FIRST_STAGE_KEY;// "Gateway1";
    public static int STAGE_LEVEL = 0;
    [field: SerializeField]
    public UserData userData
    {
        get; private set;
    }
    
    public UserStageManager userStageManager;
    public UserEquipmentManager userEquipmentManager;
    public UserSlimeManager userSlimeManager;
    public UserPurchaseManager userPurchaseManager;
    void Awake()
    {
        Application.targetFrameRate = 60;

        userData = SaveManager.LoadData<UserData>(UserDataFileName);
        if (userData == null)
        {
            userData = new UserData();
        }
        Debug.Log("UserManager Awake()");
        // userBulletManager = new UserBulletManager();
        userStageManager = new UserStageManager();
        userEquipmentManager = new UserEquipmentManager();
        userPurchaseManager = new UserPurchaseManager();
        userSlimeManager = new UserSlimeManager();

        // userBulletManager.LoadData();
        userStageManager.LoadData();
        userEquipmentManager.LoadData();
        userPurchaseManager.LoadData();
        userSlimeManager.LoadData();

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

}

[System.Serializable]
public class UserData
{
    public int gold; //던전 수정 조작 느낌으로
    public int dia; //던전 수정 조작 느낌으로

    public int GetCurrency(CurrencyType currencyType)
    {
        switch (currencyType)
        {
            case CurrencyType.Gold:
                return gold;
            case CurrencyType.Dia:
                return dia;
        }
        return 0;
    }
    public string characterName = "Lucky"; 
    public CharacterName CharacterName => Enum.Parse<CharacterName>(characterName);
}
public enum CurrencyType
{
    Gold,Dia
}