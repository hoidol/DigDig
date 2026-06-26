using UnityEngine;
using System.Collections;
using System;
using System.Globalization;

public class TimeManager : MonoSingleton<TimeManager>
{
    const string TIME_FORMAT = "yyyyMMdd HHmmss";
    public static string DateTimeToString(DateTime time)
    {
        return time.ToString(TIME_FORMAT);
    }

    public static string NowToString()
    {
        return DateTime.Now.ToString(TIME_FORMAT);
    }

    public static DateTime StringToDateTime(string str = null)
    {
        if (string.IsNullOrEmpty(str))
        {
            return DateTime.Now;
        }
        return DateTime.ParseExact(str, TIME_FORMAT, CultureInfo.InvariantCulture);
    }

    public static string FormatTime(int totalSeconds)
    {
        int days = totalSeconds / 86400; // 86400초 = 24시간 (1일)
        int hours = (totalSeconds % 86400) / 3600; // 남은 초에서 시간 계산
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        if (days > 0)
        {
            return $"{string.Format(TranslateManager.GetText("day_0"), days)}"; // 1day 02:30:45 형식
        }
        else if (hours >0)
        {         
            return $"{string.Format(TranslateManager.GetText("hour_0"), hours)} {string.Format(TranslateManager.GetText("min_0"), $"{minutes:D2}")}";
        }
        else
        {
            return $"{string.Format(TranslateManager.GetText("min_0"), $"{minutes:D2}")} {string.Format(TranslateManager.GetText("sec_0"), $"{seconds:D2}")}"; // 02:30 형식
        }
    }

    public static string FormatShortCutTime(int totalSeconds)
    {
        int days = totalSeconds / 86400; // 86400초 = 24시간 (1일)
        int hours = (totalSeconds % 86400) / 3600; // 남은 초에서 시간 계산
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        if (days > 0)
        {
            return $"{string.Format(TranslateManager.GetText("day_0"), days)}"; // 1day 02:30:45 형식
        }
        else if (hours > 0)
        {
            return $"{string.Format(TranslateManager.GetText("hour_0"), hours)} {string.Format(TranslateManager.GetText("min_0"), $"{minutes:D2}")}";
        }
        else
        {
            return $"{minutes:D2} : {seconds:D2}"; // 02:30 형식
        }
    }

    public static DateTime Now => DateTime.Now;

    static DateTime curNow;
    public static void SetServerTime(DateTime now)
    {
        curNow = now;
        lastRecordedRealtime = Time.realtimeSinceStartup;
    }


    private static float lastRecordedRealtime;

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            SyncTimeWithElapsed();
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (!isPaused)
            SyncTimeWithElapsed();
    }

    private static void SyncTimeWithElapsed()
    {
        float elapsed = Time.realtimeSinceStartup - lastRecordedRealtime;
        curNow = curNow.AddSeconds(elapsed);
        lastRecordedRealtime = Time.realtimeSinceStartup;
    }

}
