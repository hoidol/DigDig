using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
[System.Serializable]
public abstract class UserSubAchievementManager : MonoBehaviour
{
    public string UserDataFileName;

    [field: SerializeField]
    public UserAchievementData userAchievementData;
    public abstract void LoadData();


    public virtual void SaveData()
    {
        SaveManager.SaveData(UserDataFileName, userAchievementData);
    }

}


public class UserAchievementData
{

}