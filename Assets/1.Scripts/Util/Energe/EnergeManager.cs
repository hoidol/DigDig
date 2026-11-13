using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnergeManager : MonoSingleton<EnergeManager>
{
    public const int CHARGE_INTERVAL_MINUTES = 10; // 10분마다 1 충전 (MAX_ENERGE까지만, 구매·보상은 초과 가능)
    private const string LAST_CHARGE_TIME_KEY = "Energe_LastChargeTime";

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

    private void Start()
    {
        ChargeOfflineEnerge();
        ChargeLoop().Forget();
    }

    // 종료 후 재접속 시간 차이만큼 energe를 한 번에 충전
    private void ChargeOfflineEnerge()
    {
        string lastChargeTimeStr = PlayerPrefs.GetString(LAST_CHARGE_TIME_KEY, "");
        DateTime lastChargeTime = string.IsNullOrEmpty(lastChargeTimeStr)
            ? DateTime.Now
            : TimeManager.StringToDateTime(lastChargeTimeStr);

        int chargeCount = GetChargeCount((DateTime.Now - lastChargeTime).TotalMinutes);
        if (chargeCount > 0)
        {
            ChargeEnerge(chargeCount);
            lastChargeTime = lastChargeTime.AddMinutes(chargeCount * CHARGE_INTERVAL_MINUTES);
        }

        SaveLastChargeTime(lastChargeTime);
    }

    // CHARGE_INTERVAL_MINUTES마다 1씩 충전
    private async UniTaskVoid ChargeLoop()
    {
        while (true)
        {
            DateTime lastChargeTime = TimeManager.StringToDateTime(PlayerPrefs.GetString(LAST_CHARGE_TIME_KEY));
            TimeSpan wait = lastChargeTime.AddMinutes(CHARGE_INTERVAL_MINUTES) - DateTime.Now;
            if (wait > TimeSpan.Zero)
            {
                await UniTask.Delay(wait, cancellationToken: this.GetCancellationTokenOnDestroy());
            }

            lastChargeTime = lastChargeTime.AddMinutes(CHARGE_INTERVAL_MINUTES);
            ChargeEnerge(1);
            SaveLastChargeTime(lastChargeTime);
        }
    }

    // 지난 시간(분) 동안 충전되는 횟수
    public static int GetChargeCount(double elapsedMinutes) => elapsedMinutes > 0 ? (int)(elapsedMinutes / CHARGE_INTERVAL_MINUTES) : 0;

    private void ChargeEnerge(int count)
    {
        int current = UserDataManager.Instance.userData.energe;
        int addable = Mathf.Clamp(count, 0, GameSetting.MAX_ENERGE - current);
        if (addable > 0)
        {
            UserDataManager.Instance.AddEnerge(addable);

        }
    }

    private void SaveLastChargeTime(DateTime time)
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