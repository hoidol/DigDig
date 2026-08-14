using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class UserPurchaseManager : UserBaseManager
{
    public const string UserDataFileName = "UserPurchaseData";

    [field: SerializeField]
    public UserPurchaseData userPurchaseData
    {
        get; private set;
    }

    public override void LoadData()
    {
        userPurchaseData = SaveManager.LoadData<UserPurchaseData>(UserDataFileName);
        if (userPurchaseData == null)
        {
            userPurchaseData = new UserPurchaseData();
        }

        SaveData();
    }


    public UserPurchase AddUserPurchase(ProductData productData)
    {
        UserPurchase userPurchase = new UserPurchase();
        userPurchase.productId = productData.productId;
        userPurchase.key = productData.key;
        userPurchase.purchaseTime = TimeManager.NowToString();
        userPurchaseData.userPurchases.Add(userPurchase);
        SaveData();
        return userPurchase;
    }

    public override void SaveData()
    {
        SaveManager.SaveData(UserDataFileName, userPurchaseData);
    }
}



[System.Serializable]
public class UserPurchaseData
{
    //장비 보유 상태
    public List<UserPurchase> userPurchases = new List<UserPurchase>(); 
}

[System.Serializable]
public class UserPurchase
{
    public string key;
    public string productId; //유일한 값
    public string purchaseTime;    
}