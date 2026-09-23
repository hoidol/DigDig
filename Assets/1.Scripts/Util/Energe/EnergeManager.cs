using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnergeManager :MonoSingleton<EnergeManager>
{
    // public const int MAX_ENERGE = 30;
    private const int CHARGE_INTERVAL_MINUTES = 1;
    private const string LAST_CHARGE_TIME_KEY = "Energe_LastChargeTime";

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

        int elapsedMinutes = (int)(DateTime.Now - lastChargeTime).TotalMinutes;
        if (elapsedMinutes > 0)
        {
            ChargeEnerge(elapsedMinutes);
            lastChargeTime = lastChargeTime.AddMinutes(elapsedMinutes);
        }

        SaveLastChargeTime(lastChargeTime);
    }

    // 1분마다 1씩 충전
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

    private void ChargeEnerge(int count)
    {
        int current = UserDataManager.Instance.userData.energe;
        int addable = Mathf.Clamp(count, 0, GameSetting.MAX_ENERGE - current);
        if (addable > 0)
        {
            UserDataManager.Instance.AddEnerge(addable);
            GameEventBus.Publish(new ChangedEnergeEvent { energe = UserDataManager.Instance.userData.energe });
        }
    }

    private void SaveLastChargeTime(DateTime time)
    {
        PlayerPrefs.SetString(LAST_CHARGE_TIME_KEY, TimeManager.DateTimeToString(time));
    }
}

public class ChangedEnergeEvent
{
    public int energe;
    public ChangedEnergeEvent()
    {
        
    }
}