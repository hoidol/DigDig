using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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


        // userBulletManager.LoadData();
        userStageManager.LoadData();

    }


}

[System.Serializable]
public class UserData
{
    public int memoryFragmentCount; //기억의 파편 개수
}
