// (2026-11-27) 멀티 공유 글: 친구 초대(게임 링크) · 코드 공유(7자리 방 코드)
// 링크 = 스토어 주소 (Android: 앱 ID로 Google Play, iOS: IOS_APP_ID를 넣어야 App Store 주소 — 비면 Google Play 주소)
public static class MultiShare
{
    public const string IOS_APP_ID = ""; // TODO: App Store 앱 ID (숫자) — 출시 후 입력

    public static string GameLink
    {
        get
        {
#if UNITY_IOS
            if (!string.IsNullOrEmpty(IOS_APP_ID))
                return $"https://apps.apple.com/app/id{IOS_APP_ID}";
#endif
            return $"https://play.google.com/store/apps/details?id={UnityEngine.Application.identifier}";
        }
    }

    public static string InviteText => string.Format(TranslateManager.GetText("Multi_InviteText"), GameLink);
    public static string CodeText(string code) => string.Format(TranslateManager.GetText("Multi_CodeShareText"), code, GameLink);

    public static void ShareInvite() => NativeShareUtil.Share(InviteText, TranslateManager.GetText("Multi_Invite"));

    public static bool ShareCode(string code)
    {
        if (string.IsNullOrEmpty(code))
            return false;
        NativeShareUtil.Share(CodeText(code), TranslateManager.GetText("Multi_Invite"));
        return true;
    }
}
