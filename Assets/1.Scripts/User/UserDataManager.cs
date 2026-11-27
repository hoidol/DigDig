using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class UserDataManager : MonoSingleton<UserDataManager>
{
    public const string UserDataFileName = "UserData";
    // public static string STAGE_KEY = GameSetting.FIRST_STAGE_KEY;// "Gateway1";
    public bool isAdRemoved = false;
    // (2026-11-27) 멀티 판에서는 배속 상품·광고 제거 상품 효과 없음
    public bool IsGameSpeedUp => !MultiGame.IsActive && userPurchaseManager.HasPurchasedAny(InAppProductInfo.GAME_SPEED_UP_KEYS);
    public bool IsAdRemovedEffective => isAdRemoved && !MultiGame.IsActive;
    public static DifficultyType difficultyType = DifficultyType.Normal;
    public static int stageOrder = StageData.FIRST_STAGE_ORDER; // 지금 선택한 스테이지 order (0부터, 화면 표시 = +1, 로비 < > 버튼)

    [field: SerializeField]
    public UserData userData
    {
        get; private set;
    }

    public UserStageManager userStageManager;
    public UserMinionManager userMinionManager;
    public UserPurchaseManager userPurchaseManager;
    public UserMyInfoManager userMyInfoManager;
    public UserSlimeManager userSlimeManager;
    public UserHeroManager userHeroManager; // (2026-11-26) 용사 보유

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
        userPurchaseManager = new UserPurchaseManager();
        userMinionManager = new UserMinionManager();
        userMyInfoManager = new UserMyInfoManager();
        userSlimeManager = new UserSlimeManager();
        userHeroManager = new UserHeroManager();

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
        userPurchaseManager.LoadData();
        userMinionManager.LoadData();
        userMyInfoManager.LoadData();
        userSlimeManager.LoadData();
        userHeroManager.LoadData();
        // (2026-11-27) 해금 방식이 자동(BuyCurrency 빈칸)인 용사·미니언은 레벨이 되면 보유 (미니언은 데이터 로드 후 LobbyManager에서 한 번 더)
        GrantAutoUnlocks();
        isAdRemoved = userPurchaseManager.HasPurchasedAny(InAppProductInfo.REMOVE_AD_KEYS);
    }

    // 자동 해금 용사·미니언 지급 (불러올 때·유저 레벨업·로비 시작)
    public void GrantAutoUnlocks()
    {
        int lv = userMyInfoManager?.userMyInfoData != null ? userMyInfoManager.userMyInfoData.lv : 0;
        userHeroManager?.GrantAutoUnlocks(lv);
        userMinionManager?.GrantAutoUnlocks(lv);
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
            userPurchaseData = userPurchaseManager.userPurchaseData,
            userMinionData = userMinionManager.userMinionData,
            userMyInfoData = userMyInfoManager.userMyInfoData,
            userKSData = userSlimeManager.userKSData,
            userHeroData = userHeroManager.userHeroData,
        };
    }

    // 클라우드 데이터로 로컬 파일을 덮어쓰고 다시 로드. UI 갱신은 호출한 쪽에서 (씬 재로드 등)
    public void ApplyCloudData(UserCloudData data)
    {
        // 메모리의 이전 데이터가 덮어쓴 파일을 다시 덮지 않게 대기 중인 저장은 버림
        dirtySaveManagers.Clear();

        SaveIfNotNull(UserDataFileName, data.userData);
        SaveIfNotNull(UserStageManager.UserDataFileName, data.userStageData);
        SaveIfNotNull(UserPurchaseManager.UserDataFileName, data.userPurchaseData);
        SaveIfNotNull(UserMinionManager.UserDataFileName, data.userMinionData);
        SaveIfNotNull(UserMyInfoManager.UserDataFileName, data.userMyInfoData);
        SaveIfNotNull(UserSlimeManager.UserDataFileName, data.userKSData);
        SaveIfNotNull(UserHeroManager.UserDataFileName, data.userHeroData);

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
            { UserPurchaseManager.UserDataFileName, JsonUtility.ToJson(data.userPurchaseData) },
            { UserMinionManager.UserDataFileName, JsonUtility.ToJson(data.userMinionData) },
            { UserMyInfoManager.UserDataFileName, JsonUtility.ToJson(data.userMyInfoData) },
            { UserSlimeManager.UserDataFileName, JsonUtility.ToJson(data.userKSData) },
            { UserHeroManager.UserDataFileName, JsonUtility.ToJson(data.userHeroData) },
        };
    }

    // 서버에 있는 항목만 덮어씀 (없는 항목은 로컬 유지)
    public void ApplyCloudSections(IReadOnlyDictionary<string, string> sections)
    {
        ApplyCloudData(new UserCloudData
        {
            userData = FromSection<UserData>(sections, UserDataFileName),
            userStageData = FromSection<UserStageData>(sections, UserStageManager.UserDataFileName),
            userPurchaseData = FromSection<UserPurchaseData>(sections, UserPurchaseManager.UserDataFileName),
            userMinionData = FromSection<UserMinionData>(sections, UserMinionManager.UserDataFileName),
            userMyInfoData = FromSection<UserMyInfoData>(sections, UserMyInfoManager.UserDataFileName),
            userKSData = FromSection<UserKSData>(sections, UserSlimeManager.UserDataFileName),
            userHeroData = FromSection<UserHeroData>(sections, UserHeroManager.UserDataFileName),
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

    // 멀티 판 하나가 끝남 (MultiInGameSession)
    public void AddMultiRecord(bool champion, bool clear)
    {
        userData.multiGames++;
        if (clear)
            userData.multiClear++;
        if (champion)
            userData.multiChampion++;
        Save();
    }

    public void AddEnerge(int count)
    {
        userData.energe += count;
        GameEventBus.Publish(new ChangedEnergeEvent(userData.energe, count));
        Save();
    }

    // 잔액이 모자라면 false (차감 안 함). 성공 시 저장 + CurrencyPanel 갱신
    public bool TryUseCurrency(CurrencyType currencyType, int count)
    {
        if (userData.GetCurrency(currencyType) < count)
            return false;

        switch (currencyType)
        {
            case CurrencyType.Energe:
                userData.energe -= count;
                GameEventBus.Publish(new ChangedEnergeEvent(userData.energe, -count));
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
    // (2026-11-27) 골드·다이아 삭제 (이전 저장 파일의 gold·dia 값은 읽지 않고 버림)
    public int energe;
    // (2026-11-27) 멀티 전적: 우승(1등) · 클리어(웨이브 50 보스 처치) · 판수. 1등이면 우승·클리어 둘 다 +1. UI엔 우승·클리어만
    public int multiChampion;
    public int multiClear;
    public int multiGames;
    // public bool isAdRemoved;

    public int GetCurrency(CurrencyType currencyType)
    {
        switch (currencyType)
        {
            case CurrencyType.Energe:
                return energe;
        }
        return 0;
    }
    public string characterName = "Lucky";
    public SlimeName CharacterName => Enum.Parse<SlimeName>(characterName);
}
// 클라우드(Firebase) 저장 단위. 로컬 저장 파일들을 하나로 묶은 것
[System.Serializable]
public class UserCloudData
{
    public UserData userData;
    public UserStageData userStageData;
    public UserPurchaseData userPurchaseData;
    public UserMinionData userMinionData;
    public UserMyInfoData userMyInfoData;
    public UserKSData userKSData;
    public UserHeroData userHeroData; // (2026-11-26)
}

public enum CurrencyType
{
    // (2026-11-27) 골드(0)·다이아(1)·뽑기권(3) 삭제. 씬·에셋에 번호로 저장되므로 남은 값의 번호는 그대로
    Energe = 2, Ad = 4, InApp = 5
}
