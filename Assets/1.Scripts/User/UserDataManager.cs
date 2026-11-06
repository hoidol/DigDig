using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class UserDataManager : MonoSingleton<UserDataManager>
{
    public const string UserDataFileName = "UserData";
    // public static string STAGE_KEY = GameSetting.FIRST_STAGE_KEY;// "Gateway1";
    public bool isAdRemoved = false;
    public bool IsGameSpeedUp => userPurchaseManager.HasPurchased(GameSetting.GAME_SPEED_UP_KEY);
    public static DifficultyType difficultyType = DifficultyType.Normal;

    [field: SerializeField]
    public UserData userData
    {
        get; private set;
    }

    public UserStageManager userStageManager;
    public UserEquipmentManager userEquipmentManager;
    public UserSlimeManager userSlimeManager;
    public UserPurchaseManager userPurchaseManager;
    public UserMyInfoManager userMyInfoManager;
    public UserKingSlimeManager userKingSlimeManager;

    private readonly List<UserBaseManager> dirtySaveManagers = new List<UserBaseManager>();

    public void RegisterDirtySave(UserBaseManager manager)
    {
        dirtySaveManagers.Add(manager);
    }

    void LateUpdate()
    {
        FlushDirtySaves();
    }

    void FlushDirtySaves()
    {
        if (dirtySaveManagers.Count == 0)
            return;

        for (int i = 0; i < dirtySaveManagers.Count; i++)
        {
            dirtySaveManagers[i].FlushSave();
        }
        dirtySaveManagers.Clear();
    }

    void Awake()
    {
        Application.targetFrameRate = 60;

        Debug.Log("UserManager Awake()");

        userStageManager = new UserStageManager();
        userEquipmentManager = new UserEquipmentManager();
        userPurchaseManager = new UserPurchaseManager();
        userSlimeManager = new UserSlimeManager();
        userMyInfoManager = new UserMyInfoManager();
        userKingSlimeManager = new UserKingSlimeManager();

        LoadAll();
    }

    // 로컬 저장 파일에서 전부 다시 읽음 (Awake, 클라우드 불러오기 후)
    void LoadAll()
    {
        userData = SaveManager.LoadData<UserData>(UserDataFileName);
        if (userData == null)
        {
            userData = new UserData();
            userData.energe = GameSetting.MAX_ENERGE;
        }

        userStageManager.LoadData();
        userEquipmentManager.LoadData();
        userPurchaseManager.LoadData();
        userSlimeManager.LoadData();
        userMyInfoManager.LoadData();
        userKingSlimeManager.LoadData();
        isAdRemoved = userPurchaseManager.HasPurchased(GameSetting.REMOVE_AD_KEY);
    }

    // 클라우드 저장용 묶음 (FirebaseDatabase)
    public UserCloudData GetCloudData()
    {
        // 대기 중인 저장을 먼저 반영해 메모리와 파일을 맞춤
        FlushDirtySaves();
        return new UserCloudData
        {
            userData = userData,
            userStageData = userStageManager.userStageData,
            userEquipmentData = userEquipmentManager.userEquipmentData,
            userPurchaseData = userPurchaseManager.userPurchaseData,
            userSlimeData = userSlimeManager.userSlimeData,
            userMyInfoData = userMyInfoManager.userMyInfoData,
            userKSData = userKingSlimeManager.userKSData,
        };
    }

    // 클라우드 데이터로 로컬 파일을 덮어쓰고 다시 로드. UI 갱신은 호출한 쪽에서 (씬 재로드 등)
    public void ApplyCloudData(UserCloudData data)
    {
        // 메모리의 이전 데이터가 덮어쓴 파일을 다시 덮지 않게 대기 중인 저장은 버림
        dirtySaveManagers.Clear();

        SaveIfNotNull(UserDataFileName, data.userData);
        SaveIfNotNull(UserStageManager.UserDataFileName, data.userStageData);
        SaveIfNotNull(UserEquipmentManager.UserDataFileName, data.userEquipmentData);
        SaveIfNotNull(UserPurchaseManager.UserDataFileName, data.userPurchaseData);
        SaveIfNotNull(UserSlimeManager.UserDataFileName, data.userSlimeData);
        SaveIfNotNull(UserMyInfoManager.UserDataFileName, data.userMyInfoData);
        SaveIfNotNull(UserKingSlimeManager.UserDataFileName, data.userKSData);

        LoadAll();
    }

    // 클라우드 항목별 저장용 : key = 로컬 저장 파일명(각 매니저 UserDataFileName), value = JSON
    public Dictionary<string, string> GetCloudSections()
    {
        UserCloudData data = GetCloudData();
        return new Dictionary<string, string>
        {
            { UserDataFileName, JsonUtility.ToJson(data.userData) },
            { UserStageManager.UserDataFileName, JsonUtility.ToJson(data.userStageData) },
            { UserEquipmentManager.UserDataFileName, JsonUtility.ToJson(data.userEquipmentData) },
            { UserPurchaseManager.UserDataFileName, JsonUtility.ToJson(data.userPurchaseData) },
            { UserSlimeManager.UserDataFileName, JsonUtility.ToJson(data.userSlimeData) },
            { UserMyInfoManager.UserDataFileName, JsonUtility.ToJson(data.userMyInfoData) },
            { UserKingSlimeManager.UserDataFileName, JsonUtility.ToJson(data.userKSData) },
        };
    }

    // 서버에 있는 항목만 덮어씀 (없는 항목은 로컬 유지)
    public void ApplyCloudSections(IReadOnlyDictionary<string, string> sections)
    {
        ApplyCloudData(new UserCloudData
        {
            userData = FromSection<UserData>(sections, UserDataFileName),
            userStageData = FromSection<UserStageData>(sections, UserStageManager.UserDataFileName),
            userEquipmentData = FromSection<UserEquipmentData>(sections, UserEquipmentManager.UserDataFileName),
            userPurchaseData = FromSection<UserPurchaseData>(sections, UserPurchaseManager.UserDataFileName),
            userSlimeData = FromSection<UserSlimeData>(sections, UserSlimeManager.UserDataFileName),
            userMyInfoData = FromSection<UserMyInfoData>(sections, UserMyInfoManager.UserDataFileName),
            userKSData = FromSection<UserKSData>(sections, UserKingSlimeManager.UserDataFileName),
        });
    }

    static T FromSection<T>(IReadOnlyDictionary<string, string> sections, string key) where T : class
    {
        return sections.TryGetValue(key, out string json) && !string.IsNullOrEmpty(json)
            ? JsonUtility.FromJson<T>(json)
            : null;
    }

    static void SaveIfNotNull<T>(string fileName, T data) where T : class
    {
        if (data != null)
            SaveManager.SaveData(fileName, data);
    }

    void Save()
    {
        SaveManager.SaveData(UserDataFileName, userData);
    }

    // Firebase 로그인 uid 기록 (로그인할 때마다 FirebaseAuth가 맞춤). 기록용이며 서버 경로는 Firebase uid를 직접 쓴다
    public void SetUid(string uid)
    {
        if (string.IsNullOrEmpty(uid) || userData.uid == uid)
            return;
        userData.uid = uid;
        Save();
    }

    public void AddGold(int count)
    {
        userData.gold += count;
        Save();
    }
    public void AddDia(int count)
    {
        userData.dia += count;
        Save();
    }

    public void AddEnerge(int count)
    {
        userData.energe += count;
        GameEventBus.Publish(new ChangedEnergeEvent(userData.energe, count));
        Save();
    }

    public void AddDrawTicket(int drawTicket)
    {
        userData.drawTicket += drawTicket;
        Save();
    }

    // 잔액이 모자라면 false (차감 안 함). 성공 시 저장 + CurrencyPanel 갱신
    public bool TryUseCurrency(CurrencyType currencyType, int count)
    {
        if (userData.GetCurrency(currencyType) < count)
            return false;

        switch (currencyType)
        {
            case CurrencyType.Gold:
                userData.gold -= count;
                break;
            case CurrencyType.Dia:
                userData.dia -= count;
                break;
            case CurrencyType.Energe:
                userData.energe -= count;
                GameEventBus.Publish(new ChangedEnergeEvent(userData.energe, -count));
                break;
            case CurrencyType.DrawTicket:
                userData.drawTicket -= count;
                break;
        }
        Save();
        CurrencyPanel.UpdatePanels(currencyType);
        return true;
    }


}

[System.Serializable]
public class UserData
{
    public string uid;
    public int gold; //던전 수정 조작 느낌으로
    public int dia; //던전 수정 조작 느낌으로
    public int energe;
    public int drawTicket;
    // public bool isAdRemoved;

    public int GetCurrency(CurrencyType currencyType)
    {
        switch (currencyType)
        {
            case CurrencyType.Gold:
                return gold;
            case CurrencyType.Dia:
                return dia;
            case CurrencyType.Energe:
                return energe;
            case CurrencyType.DrawTicket:
                return drawTicket;
        }
        return 0;
    }
    public string characterName = "Lucky";
    public KingSlimeName CharacterName => Enum.Parse<KingSlimeName>(characterName);
}
// 클라우드(Firebase) 저장 단위. 로컬 저장 파일들을 하나로 묶은 것
[System.Serializable]
public class UserCloudData
{
    public UserData userData;
    public UserStageData userStageData;
    public UserEquipmentData userEquipmentData;
    public UserPurchaseData userPurchaseData;
    public UserSlimeData userSlimeData;
    public UserMyInfoData userMyInfoData;
    public UserKSData userKSData;
}

public enum CurrencyType
{
    Gold, Dia, Energe, DrawTicket,Ad, InApp
}
