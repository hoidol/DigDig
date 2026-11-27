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
        if (InAppProductInfo.REMOVE_AD_KEYS.Contains(key))
        {
            UserDataManager.Instance.isAdRemoved = true;
        }
        UserPurchase userPurchase = new UserPurchase();
        userPurchase.productId = productId;
        userPurchase.key = key;
        userPurchase.purchaseTime = TimeManager.NowToString();
        userPurchaseData.userPurchases.Add(userPurchase);
        RequestSave();
        if (key == InAppProductInfo.HERO_PASS_KEY) // (2026-11-27) 용사 패스 구독 결제 → 바로 구독 중 (이후 상태는 스토어 확인)
            HeroPass.SetActive(true);
        else if (key == InAppProductInfo.BASE_GAME_PACK_KEY)
            HeroPass.NotifyChanged(); // 기본 게임 팩의 용사 패스 기간 시작
        return userPurchase;
    }

    public int GetPurchaseCount(string key) => userPurchaseData.userPurchases.Count(p => p.key == key);

    // 오늘(기기 날짜 기준) 구매 횟수. 하루 제한 상품용
    public int GetTodayPurchaseCount(string key)
    {
        return userPurchaseData.userPurchases.Count(p => p.key == key && DailyCheck.IsToday(TimeManager.StringToDateTime(p.purchaseTime)));
    }
    public bool HasPurchased(string key) => userPurchaseData.userPurchases.Any(p => p.key == key );
    public bool HasPurchasedAny(string[] keys) => userPurchaseData.userPurchases.Any(p => keys.Contains(p.key));
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