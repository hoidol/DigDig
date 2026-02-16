using System;
using UnityEngine;

public class CheckNextDay : MonoBehaviour
{
    private const string LastLoginKey = "{0}_LastLoginDate"; // 마지막 접속 날짜 저장 키

    /// <summary>
    /// 새로운 날인지 확인하고, 새로운 날이면 true 반환
    /// </summary>
    public bool CheckNewDay(string key)
    {
        string storedKey = string.Format(LastLoginKey, key);
        string lastLogin = PlayerPrefs.GetString(storedKey, "");

        DateTime lastLoginDate;
        if (string.IsNullOrEmpty(lastLogin))
        {
            lastLoginDate = DateTime.MinValue; // 저장된 날짜가 없으면 최소값 설정
        }
        else
        {
            lastLoginDate = TimeManager.StringToDateTime(lastLogin);
        }

        DateTime today = DateTime.Now.Date; // 오늘 날짜 (시간 제외)

        if (lastLoginDate.Date < today) // 새로운 날이면
        {
            PlayerPrefs.SetString(storedKey, TimeManager.DateTimeToString(today)); // 오늘 날짜 저장
            return true;
        }

        return false;
    }
}
