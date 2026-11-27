using System.Runtime.InteropServices;
using UnityEngine;

// (2026-11-27) 공유 (SNS·메신저 등 기기 공유 창): Android = Intent.ACTION_SEND 선택 창, iOS = UIActivityViewController (Assets/Plugins/iOS/KSNativeShare.mm)
// 에디터·그 밖 = 클립보드에 복사 + 토스트. 외부 플러그인 없음
public static class NativeShareUtil
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void _KSShareText(string text);
#endif

    // 테스트용: 마지막으로 공유한 글
    public static string LastShared { get; private set; }

    public static void Share(string text, string title = null)
    {
        LastShared = text;
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var intentClass = new AndroidJavaClass("android.content.Intent");
            using var intent = new AndroidJavaObject("android.content.Intent");
            intent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            intent.Call<AndroidJavaObject>("setType", "text/plain");
            intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), text);
            using var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unity.GetStatic<AndroidJavaObject>("currentActivity");
            using var chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intent, title ?? "");
            activity.Call("startActivity", chooser);
            return;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Share] Android 공유 실패 → 복사 : {e.Message}");
        }
#elif UNITY_IOS && !UNITY_EDITOR
        _KSShareText(text);
        return;
#endif
        GUIUtility.systemCopyBuffer = text;
        ToastCanvas.Toast(TranslateManager.GetText("Share_Copied"));
    }
}
