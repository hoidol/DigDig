using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// (2026-11-26) 용사 보유 (UserHero.own). 저장 파일 = UserHeroData (서버 항목도 같은 이름)
// - 처음/불러올 때마다: 가격이 없는 용사(HeroData.csv BuyPrice 0)는 자동 보유. 가격이 있는 용사는 구매(BuyHero)로만
// - 상태 = 보유 / 구매 가능(유저 레벨 ≥ unnlockLv) / 잠김 (미니언 MinionBuyState와 같은 규칙, GetBuyState)
// - 구매 = HeroData.csv BuyCurrency·BuyPrice 재화 차감 → own = true → BoughtHeroEvent (상점 용사 칸에서 부를 예정)
// - 인게임 Lv.1 용사 선택 카드는 보유한 용사만 (LevelUpCanvas.MakeChoiceInput), 로비 [용사] 탭 보유 표시도 여기 기준
[System.Serializable]
public class UserHeroManager : UserBaseManager
{
    public const string UserDataFileName = "UserHeroData";

    [field: SerializeField]
    public UserHeroData userHeroData { get; private set; }

    public override void LoadData()
    {
        userHeroData = SaveManager.LoadData<UserHeroData>(UserDataFileName) ?? new UserHeroData();
        GrantFreeHeroes();
        RequestSave();
    }

    // 가격 없는 용사 자동 보유 (데이터에 새로 생긴 무료 용사도)
    void GrantFreeHeroes()
    {
        HeroDataTable table = HeroDataTable.Instance;
        if (table == null)
            return;
        foreach (HeroDataTable.HeroData hero in table.heroDatas)
            if (hero != null && !hero.hidden && hero.buyPrice <= 0)
                GetUserHero(hero.key).own = true;
    }

    public UserHero GetUserHero(string key)
    {
        UserHero hero = userHeroData.userHeroes.Find(h => h.key == key);
        if (hero == null)
        {
            hero = new UserHero { key = key };
            userHeroData.userHeroes.Add(hero);
        }
        return hero;
    }

    public bool IsOwned(string key) => !string.IsNullOrEmpty(key) && userHeroData != null
        && userHeroData.userHeroes.Exists(h => h.key == key && h.own);

    public List<string> GetOwnedKeys() => userHeroData.userHeroes.Where(h => h.own).Select(h => h.key).ToList();

    // 보유 처리 (구매·보상). 이미 있으면 그대로
    public void AddUserHero(string key)
    {
        UserHero hero = GetUserHero(key);
        if (hero.own)
            return;
        hero.own = true;
        RequestSave();
    }

    #region 구매 (상점 용사 칸)
    static int UserLevel => UserDataManager.Instance != null ? UserDataManager.Instance.userMyInfoManager.userMyInfoData.lv : 0;

    public static MinionBuyState GetBuyState(bool own, int unlockLevel, int userLv) => UserMinionManager.GetBuyState(own, unlockLevel, userLv);

    public MinionBuyState GetBuyState(string key)
    {
        HeroDataTable.HeroData data = HeroDataTable.Get(key);
        return GetBuyState(IsOwned(key), data != null ? data.unlockLv : 0, UserLevel);
    }

    public MinionBuyResult BuyHero(string key)
    {
        HeroDataTable.HeroData data = HeroDataTable.Get(key);
        if (data == null || data.hidden)
            return MinionBuyResult.Locked;
        MinionBuyState state = GetBuyState(key);
        if (state == MinionBuyState.Owned)
            return MinionBuyResult.AlreadyOwned;
        if (state == MinionBuyState.Locked)
            return MinionBuyResult.Locked;
        if (data.buyPrice > 0 && !UserDataManager.Instance.TryUseCurrency(data.buyCurrency, data.buyPrice))
            return MinionBuyResult.NotEnoughCurrency;
        AddUserHero(key);
        GameEventBus.Publish(new BoughtHeroEvent(key));
        return MinionBuyResult.Success;
    }
    #endregion

    public override void SaveData()
    {
        SaveManager.SaveData(UserDataFileName, userHeroData);
    }
}

[System.Serializable]
public class UserHeroData
{
    public List<UserHero> userHeroes = new List<UserHero>();
}

[System.Serializable]
public class UserHero
{
    public string key;
    public bool own;
}

public class BoughtHeroEvent
{
    public string key;
    public BoughtHeroEvent(string key) { this.key = key; }
}
