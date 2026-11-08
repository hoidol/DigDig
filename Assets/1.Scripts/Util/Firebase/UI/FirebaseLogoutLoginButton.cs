using UnityEngine;

// 로그아웃. 게스트 계정은 로그아웃하면 다시 찾을 수 없으므로 경고 후 확인
public class FirebaseLogoutLoginButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        FirebaseAuth auth = FirebaseManager.Instance != null ? FirebaseManager.Instance.firebaseAuth : null;
        if (auth == null || !auth.IsSignedIn)
            return;

        string body = auth.CurrentLoginType == LoginType.Anonymous
            ? TranslateManager.GetText("Logout_Guest_Confirm")
            : TranslateManager.GetText("Logout_Confirm");

        YesOrNoCanvas.Instance.OpenCanvas(TranslateManager.GetText("Logout"), body, yes =>
        {
            if (!yes)
                return;
            auth.SignOut();
            ToastCanvas.Toast(TranslateManager.GetText("Logout_Done"));
        });
    }
}
