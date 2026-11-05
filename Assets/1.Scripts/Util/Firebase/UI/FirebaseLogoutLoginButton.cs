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
            ? "게스트 계정은 로그아웃하면 다시 불러올 수 없습니다.\n로그아웃할까요?"
            : "로그아웃할까요?";

        YesOrNoCanvas.Instance.OpenCanvas("로그아웃", body, yes =>
        {
            if (!yes)
                return;
            auth.SignOut();
            ToastCanvas.Toast("로그아웃 완료");
        });
    }
}
