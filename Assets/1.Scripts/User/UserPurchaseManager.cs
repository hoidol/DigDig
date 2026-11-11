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

        RequestSave();
    }


    public UserPurchase AddUserPurchase(ProductData productData) => AddUserPurchase(productData.key, productData.productId);

    public UserPurchase AddUserPurchase(string key, string productId = null)
    {
        if(key == InAppProductInfo.REMOVE_AD_KEY)
        {
            UserDataManager.Instance.isAdRemoved = true;
        }
        UserPurchase userPurchase = new UserPurchase();
        userPurchase.productId = productId;
        userPurchase.key = key;
        userPurchase.purchaseTime = TimeManager.NowToString();
        userPurchaseData.userPurchases.Add(userPurchase);
        RequestSave();
        return userPurchase;
    }

    public int GetPurchaseCount(string key) => userPurchaseData.userPurchases.Count(p => p.key == key);

    // 오늘(기기 날짜 기준) 구매 횟수. 하루 제한 상품용
    public int GetTodayPurchaseCount(string key)
    {
        System.DateTime today = TimeManager.Now.Date;
        return userPurchaseData.userPurchases.Count(p => p.key == key && TimeManager.StringToDateTime(p.purchaseTime).Date == today);
    }
    public bool HasPurchased(string key) => userPurchaseData.userPurchases.Any(p => p.key == key );
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