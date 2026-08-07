using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class UserEquipmentManager : UserBaseManager
{
    public const string UserDataFileName = "UserEquipmentData";

    [field: SerializeField]
    public UserEquipmentData userEquipmentData
    {
        get; private set;
    }

    public override void LoadData()
    {
        userEquipmentData = SaveManager.LoadData<UserEquipmentData>(UserDataFileName);
        if (userEquipmentData == null)
        {
            userEquipmentData = new UserEquipmentData();
        }

        SaveData();
    }


    public UserEquipment GetUserEquipment(string id)
    {
        return userEquipmentData.userEquipments.Where(e => e.id == id).FirstOrDefault();
    }

    public UserEquipment AddUserEquipment(string key)
    {
        UserEquipment userEquipment = new UserEquipment();
        userEquipment.id = System.Guid.NewGuid().ToString();
        userEquipment.key = key;
        userEquipment.equipped = false;
        userEquipmentData.userEquipments.Add(userEquipment);
        // userEquipmentData.userEquipments = userEquipmentData.userEquipments.OrderByDescending(e => e.equipmentData.grade).ThenBy(e => e.equipmentData.equipmentType).ThenBy(e => e.equipmentData.key).ToList();
        SaveData();
        return userEquipment;
    }

    public void RemoveUserEquipment(UserEquipment userEquipment)
    {
        RemoveUserEquipment(userEquipment.id);
    }

    public void RemoveUserEquipment(string id)
    {
        UserEquipment userEquipment = GetUserEquipment(id);
        if(userEquipment == null)
        {
            Debug.Log($"RemoveUserEquipment {id} 장비 없음");
            return;
        }
        userEquipmentData.userEquipments.Remove(userEquipment);
        SaveData();
    }

    public UserEquipment EquiptUserEquipment(UserEquipment userEquipment)
    {
        ReleaseUserEquipment(userEquipment.key);
        
        userEquipment.equipped = true;

        SaveData();
        return userEquipment;
    }
    public UserEquipment GetEquippedUserEquipment(EquipmentType equipmentType)
    {
        return userEquipmentData.userEquipments.Where(e=> e.equipmentData.equipmentType == equipmentType && e.equipped == true).FirstOrDefault();
    }

    public UserEquipment ReleaseUserEquipment(EquipmentType equipmentType)
    {
        UserEquipment userEquipment = GetEquippedUserEquipment(equipmentType);
        if(userEquipment == null)
        {
            Debug.Log($"ReleaseUserEquipment {equipmentType} 장비 없음");
            return null;
        }
        
        return ReleaseUserEquipment(userEquipment.id);
    }

    public UserEquipment ReleaseUserEquipment(string id)
    {
        UserEquipment userEquipment = GetUserEquipment(id);
        userEquipment.equipped = false;

        SaveData();
        return userEquipment;
    }
    public UserEquipment[] GetEquippedUserEquipments()
    {
        return userEquipmentData.userEquipments.Where(e => e.equipped == true).ToArray();
    }

    public override void SaveData()
    {
        SaveManager.SaveData(UserDataFileName, userEquipmentData);
    }
}



[System.Serializable]
public class UserEquipmentData
{
    //장비 보유 상태
    public List<UserEquipment> userEquipments = new List<UserEquipment>(); 
}

[System.Serializable]
public class UserEquipment
{
    public string id; //유일한 값
    public string key;
    public bool equipped;
    public EquipmentData equipmentData
    {
        get
        {
            return EquipmentManager.Instance.GetEquipmentData(key);
        }
    }
    
}