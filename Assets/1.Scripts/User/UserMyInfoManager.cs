using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class UserMyInfoManager : UserBaseManager
{
    public const string UserDataFileName = "UserMyInfoData";

    [field: SerializeField]
    public UserMyInfoData userMyInfoData
    {
        get; private set;
    }

    // 유저 최대 레벨. 업데이트로 올릴 수 있음 (최대 레벨 동안 경험치는 0으로 유지 → 올리면 0부터 다시 쌓임)
    public const int MAX_LEVEL = 30;

    public static bool IsMaxLevel(int lv) => lv >= MAX_LEVEL;

    // 받을 경험치. 최대 레벨이면 0
    public static int GetGainExp(int lv, int exp) => IsMaxLevel(lv) ? 0 : Mathf.Max(0, exp);

    public bool IsMaxLv => IsMaxLevel(userMyInfoData.lv);

    // 인게임 결과로 받았지만 로비 경험치 바에 아직 안 보여준 경험치 (로비 진입 시 바가 차오르는 연출에 쓰고 0으로. 저장 안 함)
    public static int unshownGainExp;

    // 마지막 LevelUp에서 새로 받은 미니언 key (저장 안 함)
    [System.NonSerialized] public List<string> lastUnlockedMinionKeys = new List<string>();

    public override void LoadData()
    {
        userMyInfoData = SaveManager.LoadData<UserMyInfoData>(UserDataFileName);
        if(userMyInfoData == null)
        {
            userMyInfoData= new UserMyInfoData();
            userMyInfoData.nickname = System.Guid.NewGuid().ToString();
        }

        if (IsMaxLv)
            userMyInfoData.exp = 0;

        RequestSave();
    }
    public override void SaveData()
    {
        SaveManager.SaveData(UserDataFileName, userMyInfoData);
    }


    // ---- (2026-11-27) 닉네임 (Title 씬에서 처음 정함)
    public const int NICKNAME_MIN = 2;
    public const int NICKNAME_MAX = 10;

    // 아직 안 정함 = 처음 만든 기본값(GUID)
    public bool HasNickname => !string.IsNullOrWhiteSpace(userMyInfoData.nickname) && !System.Guid.TryParse(userMyInfoData.nickname, out _);

    // 닉네임 규칙: 앞뒤 공백 정리 · 2~10자 · (2026-11-27) 한글·영문·숫자만 (공백·특수문자 X) · 금칙어 X (씬 없이 계산). 문제가 없으면 null, 있으면 번역 key
    static readonly System.Text.RegularExpressions.Regex NICKNAME_CHARS = new System.Text.RegularExpressions.Regex("^[가-힣A-Za-z0-9]+$");
    public static string ValidateNickname(string raw, out string nickname)
    {
        nickname = (raw ?? "").Trim();
        if (nickname.Length < NICKNAME_MIN || nickname.Length > NICKNAME_MAX)
            return "Nickname_Length";
        if (nickname.Contains('\n') || nickname.Contains('\t'))
            return "Nickname_Invalid";
        if (!NICKNAME_CHARS.IsMatch(nickname)) // 특수문자·공백·자모만 X (서버 key로 못 쓰는 글자도 여기서 걸림)
            return "Nickname_SpecialChar";
        if (System.Guid.TryParse(nickname, out _))
            return "Nickname_Invalid";
        if (ClearTipManager.MaskNow(nickname) != nickname)
            return "Nickname_Banned";
        return null;
    }

    public bool SetNickname(string raw)
    {
        if (ValidateNickname(raw, out string nickname) != null)
            return false;
        if (userMyInfoData.nickname != nickname)
            userMyInfoData.nickChangeCount++;
        userMyInfoData.nickname = nickname;
        RequestSave();
        GameEventBus.Publish(new ChangedUserExpEvent()); // MyInfoPanel 다시 그림 (닉네임 표시)
        return true;
    }

    public void AddExp(int exp)
    {
        exp = GetGainExp(userMyInfoData.lv, exp);
        if (exp <= 0)
            return;

        userMyInfoData.exp += exp;
        RequestSave();
        GameEventBus.Publish(new ChangedUserExpEvent());
    }

    public int GetMaxExp(int lv = -1)
    {
        if (lv < 0)
        {
            lv = userMyInfoData.lv;
        }

        return 100 + (lv * 25);
    }

    public bool CheckLevelUp()
    {
        if (IsMaxLv)
            return false;

        int remainExp = userMyInfoData.exp - GetMaxExp(userMyInfoData.lv);
        if (remainExp >= 0)
        {
            return true;
        }
        return false;
    }

    public bool LevelUp()
    {
        if (IsMaxLv)
            return false;

        int remainExp = userMyInfoData.exp - GetMaxExp(userMyInfoData.lv);
        if (remainExp >= 0)
        {
            userMyInfoData.lv++;
            // 최대 레벨에 도달하면 남은 경험치는 버림
            userMyInfoData.exp = IsMaxLv ? 0 : remainExp;
            // 이 레벨에 해금된 미니언 (로비 레벨업 창 뒤에 토스트). (2026-11-27) 자동 해금 용사·미니언은 여기서 바로 보유
            lastUnlockedMinionKeys = new List<string>(UserMinionManager.GetLevelUnlockKeysAt(userMyInfoData.lv));
            UserDataManager.Instance?.GrantAutoUnlocks();
            RequestSave();
            GameEventBus.Publish(new ChangedUserExpEvent());
            return true;
        }
        return false;
    }
}



// 유저 경험치/레벨 변경 시 발행 (MyInfoPanel 갱신)
public class ChangedUserExpEvent { }

[System.Serializable]
public class UserMyInfoData
{
    public int exp;
    public int lv;
    public string nickname;
    public int nickChangeCount;
}
