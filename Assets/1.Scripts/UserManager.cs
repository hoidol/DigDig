using System.Collections.Generic;
using UnityEngine;

public class UserManager : MonoSingleton<UserManager>
{
    public static string stageKey = "Greed";
#if UNITY_EDITOR
    string[] bulletKeys = new string[]
   {
      "Pierce","Flame","Giant","Thunder","Iron"
   };
#endif
    [field: SerializeField]
    public UserData userData
    {
        get; private set;
    }
    void Awake()
    {
        Application.targetFrameRate = 60;

        userData = SaveManager.LoadData<UserData>("UserData");
        if (userData == null)
        {
            userData = new UserData();
            for (int i = 0; i < bulletKeys.Length; i++)
            {
                userData.equiptedBullets[i] = new UserBullet
                {
                    key = bulletKeys[i],
                    equiped = i
                };
            }

        }
    }
}

[System.Serializable]
public class UserData
{
    public UserBullet[] equiptedBullets = new UserBullet[5];
    public List<UserBullet> userBullets = new List<UserBullet>(); //바ㅇ
}

[System.Serializable]
public class UserBullet
{
    public string key;
    public int equiped = -1;
    public int lv;
}