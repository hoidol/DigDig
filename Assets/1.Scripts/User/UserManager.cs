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
    // public UserBulletManager userBulletManager;
    public UserStageManager userStageManager;
    public UserEquipmentManager userEquipmentManager;
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

        // userBulletManager.LoadData();
        userStageManager.LoadData();
        userEquipmentManager.LoadData();

    }
    void Save()
    {
        SaveManager.SaveData(UserDataFileName, userData); 
    }
    public void AddMemoryPiece(int count)
    {
        userData.memoryPieceCount += count;
        Save();
    }

}

[System.Serializable]
public class UserData
{
    public int memoryPieceCount; //기억의 조각 개수
    public string characterName = "Lucky"; 
    public CharacterName CharacterName => Enum.Parse<CharacterName>(characterName);
}
