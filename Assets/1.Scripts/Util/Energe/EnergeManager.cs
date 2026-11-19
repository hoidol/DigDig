using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnergeManager : MonoSingleton<EnergeManager>
{
    public const int CHARGE_INTERVAL_MINUTES = 5; // 5분마다 1 충전 (MAX_ENERGE까지만, 구매·보상은 초과 가능)
    private const string LAST_CHARGE_TIME_KEY = "Energe_LastChargeTime";
    private const float CHECK_INTERVAL_SECONDS = 1f; // 켜져 있는 동안 충전 확인 주기 (실제 시간)

    // BuildSetting.ignoreEnerge: 에너지를 항상 최대로 취급 (차감 안 함, 저장값은 건드리지 않음)
    public static bool DisableIgnoreForTest; // 테스트에서는 설정과 상관없이 실제 차감
    public static bool IsIgnored => !DisableIgnoreForTest && BuildSetting.Instance != null && BuildSetting.Instance.ignoreEnerge;

    // 화면 표시·시작 가능 판단에 쓰는 에너지
    public static int Current => IsIgnored ? Mathf.Max(GameSetting.MAX_ENERGE, UserDataManager.Instance.userData.energe) : UserDataManager.Instance.userData.energe;

    public static bool CanPlay(int energe) => energe >= GameSetting.PLAY_COST_ENERGE;

    // 게임 시작 비용 차감. 모자라면 false (차감 안 함)
    public static bool TryUsePlayCost()
    {
        if (IsIgnored)
            return true;
        if (!CanPlay(UserDataManager.Instance.userData.energe))
            return false;
        UserDataManager.Instance.AddEnerge(-GameSetting.PLAY_COST_ENERGE);
        return true;
    }

    // 다음 1 충전까지 남은 시간 (최대면 Zero)
    public static TimeSpan RemainTimeToNextCharge
    {
        get
        {
            if (UserDataManager.Instance.userData.energe >= GameSetting.MAX_ENERGE)
                return TimeSpan.Zero;
            TimeSpan remain = LoadLastChargeTime().AddMinutes(CHARGE_INTERVAL_MINUTES) - DateTime.Now;
            return remain > TimeSpan.Zero ? remain : TimeSpan.Zero;
        }
    }

    bool isAway; // 앱이 꺼져 있거나 포커스를 잃은 상태였는지

    private void Start()
    {
        // 앱을 껐다 켠 경우: 꺼져 있던 시간만큼 충전 + 알림
        ChargeByElapsedTime(true);
        ChargeLoop().Forget();
    }

    // 백그라운드로 갔다가(포커스 잃음) 돌아온 경우: 흐른 시간만큼 충전 + 알림
    private void OnApplicationFocus(bool hasFocus) => OnAwayChanged(!hasFocus);
    private void OnApplicationPause(bool paused) => OnAwayChanged(paused);

    void OnAwayChanged(bool away)
    {
        if (away)
        {
            if (isAway)
                return;
            isAway = true;
            ChargeByElapsedTime(false); // 나가기 직전 상태를 기록
            PlayerPrefs.Save();         // 백그라운드에서 강제 종료돼도 기록이 남게
            return;
        }

        if (!isAway)
            return;
        isAway = false;
        ChargeByElapsedTime(true);
    }

    // 켜져 있는 동안: 실제 시간 기준으로 확인 (timeScale·배속과 무관)
    private async UniTaskVoid ChargeLoop()
    {
        var token = this.GetCancellationTokenOnDestroy();
        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(CHECK_INTERVAL_SECONDS), DelayType.Realtime, cancellationToken: token);
            if (!isAway)
                ChargeByElapsedTime(false);
        }
    }

    // 마지막 충전 시각부터 흐른 시간만큼 충전. 충전한 개수 반환
    int ChargeByElapsedTime(bool notify)
    {
        if (UserDataManager.Instance == null || UserDataManager.Instance.userData == null)
            return 0;

        DateTime now = DateTime.Now;
        DateTime lastChargeTime = LoadLastChargeTime();
        int current = UserDataManager.Instance.userData.energe;

        // 처음 실행이거나 기기 시간을 되돌린 경우: 지금부터 다시 셈
        if (!PlayerPrefs.HasKey(LAST_CHARGE_TIME_KEY) || lastChargeTime > now)
        {
            SaveLastChargeTime(now);
            return 0;
        }

        // 최대(이상)면 충전 없음, 타이머는 에너지를 쓴 시점부터 시작되게 계속 지금으로
        if (current >= GameSetting.MAX_ENERGE)
        {
            SaveLastChargeTime(now);
            return 0;
        }

        int chargeCount = GetChargeCount((now - lastChargeTime).TotalMinutes);
        if (chargeCount <= 0)
            return 0;

        int added = GetAddableCount(current, chargeCount);
        if (added > 0)
            UserDataManager.Instance.AddEnerge(added);

        // 최대까지 찼으면 남은 시간은 버림, 아니면 남은 분은 다음 충전으로 이어짐
        if (current + added >= GameSetting.MAX_ENERGE)
            SaveLastChargeTime(now);
        else
            SaveLastChargeTime(lastChargeTime.AddMinutes(chargeCount * CHARGE_INTERVAL_MINUTES));

        if (notify && added > 0)
            ShowChargedToast(added);
        return added;
    }

    static void ShowChargedToast(int added)
    {
        if (ToastCanvas.Instance == null)
            return;
        ToastCanvas.Toast(string.Format(TranslateManager.GetText("Energe_OfflineCharged"), added));
    }

    // 지난 시간(분) 동안 충전되는 횟수
    public static int GetChargeCount(double elapsedMinutes) => elapsedMinutes > 0 ? (int)(elapsedMinutes / CHARGE_INTERVAL_MINUTES) : 0;

    // 자동 충전으로 실제 더해지는 양 (MAX_ENERGE까지만)
    public static int GetAddableCount(int current, int chargeCount) => Mathf.Clamp(chargeCount, 0, Mathf.Max(0, GameSetting.MAX_ENERGE - current));

    static DateTime LoadLastChargeTime() => TimeManager.StringToDateTime(PlayerPrefs.GetString(LAST_CHARGE_TIME_KEY, ""));

    static void SaveLastChargeTime(DateTime time)
    {
        PlayerPrefs.SetString(LAST_CHARGE_TIME_KEY, TimeManager.DateTimeToString(time));
    }
}

public class ChangedEnergeEvent
{
    public int totalEnerge;
    public int change;//변화량
    public ChangedEnergeEvent(int totalEnerge, int change)
    {
        this.totalEnerge = totalEnerge;
        this.change = change;
    }
}
