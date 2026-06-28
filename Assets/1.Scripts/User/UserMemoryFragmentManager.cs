using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[System.Serializable]
public class UserMemoryFragmentManager : UserBaseManager
{
    public const string UserDataFileName = "UserMemoryFragmentData";

    [field: SerializeField]
    public UserMemoryFragmentData userMemoryFragmentData
    {
        get; private set;
    }

    public override void LoadData()
    {
        userMemoryFragmentData = SaveManager.LoadData<UserMemoryFragmentData>(UserDataFileName);
        if (userMemoryFragmentData == null)
        {
            userMemoryFragmentData = new UserMemoryFragmentData();
            userMemoryFragmentData.userMemoryFragmentGroups = new UserMemoryFragmentGroup[4];
            for (int i = 0; i < userMemoryFragmentData.userMemoryFragmentGroups.Length; i++)
            {
                userMemoryFragmentData.userMemoryFragmentGroups[i] = new UserMemoryFragmentGroup();
                userMemoryFragmentData.userMemoryFragmentGroups[i].groupLevel = i;
            }
        }
    }

    public override void SaveData()
    {
        SaveManager.SaveData(UserDataFileName, userMemoryFragmentData);
    }

    public void UpgradeAbiity(string key)
    {
        UserMemoryFragment userMemoryFragment = GetUserMemoryFragment(key);
        MemoryFragmentAbilityData abilityData = MemoryFragmentManager.Instance.GetMemoryFragmentAbilityData(key);
        UserMemoryFragmentGroup group = GetUserMemoryFragmentGroup(abilityData.level);
        group.point++;
        userMemoryFragment.point++;

        SaveData();
    }

    UserMemoryFragment GetUserMemoryFragment(string key)
    {
        UserMemoryFragment uMF = userMemoryFragmentData.userMemoryFragments.Where(e => e.key == key).FirstOrDefault();
        if (uMF == null)
        {
            uMF = new UserMemoryFragment();
            uMF.key = key;
            userMemoryFragmentData.userMemoryFragments.Add(uMF);
        }
        return uMF;
    }

    UserMemoryFragmentGroup GetUserMemoryFragmentGroup(int lv)
    {
        return userMemoryFragmentData.userMemoryFragmentGroups.Where(e => e.groupLevel == lv).FirstOrDefault();
    }
}
public class UserMemoryFragmentData
{
    public UserMemoryFragmentGroup[] userMemoryFragmentGroups;//= new UserMemoryFragmentGroup[4];
    public List<UserMemoryFragment> userMemoryFragments = new List<UserMemoryFragment>();

}

public class UserMemoryFragmentGroup
{
    public int groupLevel;
    public int point;
}

public class UserMemoryFragment
{
    public string key;
    public int point;
}