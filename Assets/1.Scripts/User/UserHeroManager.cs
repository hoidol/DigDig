using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// (2026-11-26) 용사 보유 (UserHero.own). 저장 파일 = UserHeroData (서버 항목도 같은 이름)
// - (2026-11-27) HeroData.csv isFree TRUE = 유저 레벨 ≥ unnlockLv면 무료 보유(own, 불러올 때·레벨업·로비 시작)
//   isFree가 아닌 용사 = 용사 패스(월 구독, HeroPass)가 있는 동안만 이용 (own에 저장하지 않음 → 구독이 끝나면 바로 못 씀, 레벨 조건 없음)
// - 상태 = 보유(이용 가능) / 해금 가능(패스 필요) / 잠김(무료 용사 레벨 미달) (미니언 MinionBuyState와 같은 이름, GetBuyState)
// - UnlockHero = 패스 용사면 용사 패스 구독 결제 → 성공하면 모든 패스 용사 이용 → BoughtHeroEvent
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
        RequestSave(); // 자동 해금은 UserDataManager.LoadAll 끝에서 (유저 레벨을 읽은 뒤)
    }

    // 자동 해금 용사 중 레벨이 된 것 보유 (데이터에 새로 생긴 용사도)
    public void GrantAutoUnlocks(int userLv)
    {
        HeroDataTable table = HeroDataTable.Instance;
        if (table == null || userHeroData == null)
            return;
        bool changed = false;
        foreach (HeroDataTable.HeroData hero in table.heroDatas)
        {
            if (hero == null || hero.hidden || !hero.isFree || userLv < hero.unlockLv)
                continue;
            UserHero user = GetUserHero(hero.key);
            if (user.own)
                continue;
            user.own = true;
            changed = true;
        }
        if (changed)
            RequestSave();
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

    // 이용 가능 = 무료 보유(own) 또는 (패스 용사 + 용사 패스 구독 중)
    public bool IsOwned(string key)
    {
        if (string.IsNullOrEmpty(key) || userHeroData == null)
            return false;
        if (userHeroData.userHeroes.Exists(h => h.key == key && h.own))
            return true;
        HeroDataTable.HeroData data = HeroDataTable.Get(key);
        return data != null && !data.hidden && !data.isFree && HeroPass.IsActive;
    }

    public List<string> GetOwnedKeys()
    {
        HeroDataTable table = HeroDataTable.Instance;
        if (table == null)
            return userHeroData.userHeroes.Where(h => h.own).Select(h => h.key).ToList();
        return table.heroDatas.Where(h => h != null && IsOwned(h.key)).Select(h => h.key).ToList();
    }

    // 보유 처리 (구매·보상). 이미 있으면 그대로
    public void AddUserHero(string key)
    {
        UserHero hero = GetUserHero(key);
        if (hero.own)
            return;
        hero.own = true;
        RequestSave();
    }

    #region 해금 (상점 용사 칸)
    static int UserLevel => UserDataManager.Instance != null ? UserDataManager.Instance.userMyInfoManager.userMyInfoData.lv : 0;

    public static MinionBuyState GetBuyState(bool own, int unlockLevel, int userLv) => UserMinionManager.GetBuyState(own, unlockLevel, userLv);

    public MinionBuyState GetBuyState(string key)
    {
        HeroDataTable.HeroData data = HeroDataTable.Get(key);
        if (data == null || data.hidden)
            return MinionBuyState.Locked;
        if (IsOwned(key))
            return MinionBuyState.Owned;
        return data.isFree ? GetBuyState(false, data.unlockLv, UserLevel) : MinionBuyState.Buyable; // 패스 용사 = 패스 필요
    }

    // 해금: 무료 용사(레벨 도달) = 바로 보유 / 패스 용사 = 용사 패스 구독 결제 → 성공하면 이용. 결과는 done으로 (결제는 나중에 옴)
    public void UnlockHero(string key, System.Action<MinionBuyResult> done)
    {
        HeroDataTable.HeroData data = HeroDataTable.Get(key);
        MinionBuyState state = data == null || data.hidden ? MinionBuyState.Locked : GetBuyState(key);
        if (state != MinionBuyState.Buyable)
        {
            done?.Invoke(state == MinionBuyState.Owned ? MinionBuyResult.AlreadyOwned : MinionBuyResult.Locked);
            return;
        }
        if (data.isFree)
        {
            AddUserHero(key);
            GameEventBus.Publish(new BoughtHeroEvent(key));
            done?.Invoke(MinionBuyResult.Success);
            return;
        }
        HeroPass.Buy(ok =>
        {
            if (ok)
                GameEventBus.Publish(new BoughtHeroEvent(key));
            done?.Invoke(ok ? MinionBuyResult.Success : MinionBuyResult.Cancelled);
        });
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
