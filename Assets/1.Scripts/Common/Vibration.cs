using UnityEngine;
using CandyCoded.HapticFeedback;
public static class Vibration
{
    private const string VibrationPrefKey = "VibrationEnabled";
    private static bool isVibrationEnabled;

    static Vibration()
    {
        LoadVibrationSetting();
    }

    public static void SetVibrationEnabled(bool enabled)
    {
        isVibrationEnabled = enabled;
        PlayerPrefs.SetInt(VibrationPrefKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private static void LoadVibrationSetting()
    {
        isVibrationEnabled = PlayerPrefs.GetInt(VibrationPrefKey, 1) == 1;
    }

    public static bool IsVibrationEnabled()
    {
        return isVibrationEnabled;
    }

    public static void Vibrate(int style)
    {
        if (!isVibrationEnabled) return; // 진동 설정이 꺼져있으면 리턴


#if !UNITY_EDITOR
        switch (style)
        {

            case 1:
                HapticFeedback.MediumFeedback();
                break;

            case 2:
                HapticFeedback.HeavyFeedback();
                break;
            default:
                HapticFeedback.LightFeedback();
                break;

        }
#else
        //Debug.Log("진동은 실제 기기에서만 작동합니다.");
#endif
    }
}
