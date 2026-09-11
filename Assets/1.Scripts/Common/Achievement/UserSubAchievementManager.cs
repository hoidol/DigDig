using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class UserSubAchievementManager 
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