using System;
using UnityEngine;

// "하루에 한 번" 처리 공용: 날짜가 바뀌었는지 판단은 전부 여기로 (기기 시간 자정 기준)
// 사용: 저장해 둔 날짜 문자열을 IsToday로 비교하고, 처리한 뒤 Today를 저장한다
//   - 저장 데이터에 날짜를 두는 경우: IsToday(data.date) / data.date = Today
//   - PlayerPrefs에 두는 경우: IsTodayPrefs(key) / MarkTodayPrefs(key)
public static class DailyCheck
{
    const string DATE_FORMAT = "yyyyMMdd";

    // 테스트용: 값을 넣으면 그 시각을 현재로 취급 (기본 null = 기기 시간)
    public static DateTime? NowForTest;

    public static DateTime Now => NowForTest ?? TimeManager.Now;

    // 오늘 날짜 (yyyyMMdd)
    public static string Today => Now.ToString(DATE_FORMAT);

    // 저장해 둔 날짜가 오늘인지 (비어 있으면 false)
    public static bool IsToday(string date) => !string.IsNullOrEmpty(date) && date == Today;

    // 그 시각이 오늘인지
    public static bool IsToday(DateTime time) => time.Date == Now.Date;

    public static bool IsTodayPrefs(string prefsKey) => IsToday(PlayerPrefs.GetString(prefsKey, ""));

    public static void MarkTodayPrefs(string prefsKey) => PlayerPrefs.SetString(prefsKey, Today);
}
