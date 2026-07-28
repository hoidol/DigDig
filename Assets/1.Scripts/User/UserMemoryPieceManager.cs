using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[System.Serializable]
public class UserMemoryPieceManager : UserBaseManager
{
    public const string UserDataFileName = "UserMemoryPieceData";

    [field: SerializeField]
    public UserMemoryPieceData userMemoryPieceData
    {
        get; private set;
    }

    public override void LoadData()
    {
        userMemoryPieceData = SaveManager.LoadData<UserMemoryPieceData>(UserDataFileName);
        if (userMemoryPieceData == null)
        {
            userMemoryPieceData = new UserMemoryPieceData();
            userMemoryPieceData.userMemoryPieceGroups = new UserMemoryPieceGroup[4];
            for (int i = 0; i < userMemoryPieceData.userMemoryPieceGroups.Length; i++)
            {
                userMemoryPieceData.userMemoryPieceGroups[i] = new UserMemoryPieceGroup();
                userMemoryPieceData.userMemoryPieceGroups[i].groupLevel = i;
            }
        }
    }

    public override void SaveData()
    {
        SaveManager.SaveData(UserDataFileName, userMemoryPieceData);
    }

    public void UpgradeAbiity(string key)
    {
        UserMemoryPiece userMemoryPiece = GetUserMemoryPiece(key);
        MemoryPieceAbilityData abilityData = MemoryPieceManager.Instance.GetMemoryPieceAbilityData(key);
        UserMemoryPieceGroup group = GetUserMemoryPieceGroup(abilityData.level);
        group.point++;
        userMemoryPiece.point++;

        SaveData();
    }

    UserMemoryPiece GetUserMemoryPiece(string key)
    {
        UserMemoryPiece uMF = userMemoryPieceData.userMemoryPieces.Where(e => e.key == key).FirstOrDefault();
        if (uMF == null)
        {
            uMF = new UserMemoryPiece();
            uMF.key = key;
            userMemoryPieceData.userMemoryPieces.Add(uMF);
        }
        return uMF;
    }

    UserMemoryPieceGroup GetUserMemoryPieceGroup(int lv)
    {
        return userMemoryPieceData.userMemoryPieceGroups.Where(e => e.groupLevel == lv).FirstOrDefault();
    }
}
public class UserMemoryPieceData
{
    public UserMemoryPieceGroup[] userMemoryPieceGroups;//= new UserMemoryPieceGroup[4];
    public List<UserMemoryPiece> userMemoryPieces = new List<UserMemoryPiece>();

}

public class UserMemoryPieceGroup
{
    public int groupLevel;
    public int point;
}

public class UserMemoryPiece
{
    public string key;
    public int point;
}