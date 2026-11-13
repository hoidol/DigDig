using Firebase.Auth;

// 로그인돼 있으면 바로 RankingCanvas, 아니면 설정 창(SettingCanvas, 로그인) → 로그인 성공 시 RankingCanvas
public class RankingButton : ButtonUI
{
    bool waitingLogin;

    public override void OnClickedBtn()
    {
        if (RankingManager.IsTestMode) // 테스트: 로그인 확인 없이 바로
        {
            RankingCanvas.Instance.OpenCanvas();
            return;
        }

        FirebaseAuth auth = FirebaseManager.Instance != null ? FirebaseManager.Instance.firebaseAuth : null;
        if (auth == null || !auth.IsReady)
        {
            ToastCanvas.Toast(TranslateManager.GetText("Server_Connecting_Retry"));
            return;
        }

        if (auth.IsSignedIn)
        {
            RankingCanvas.Instance.OpenCanvas();
            return;
        }

        if (waitingLogin)
            return;
        waitingLogin = true;
        FirebaseAuth.OnSignedIn += OnSignedIn;
        SettingCanvas.Instance.OpenCanvas(StopWaitingLogin); // 로그인 없이 닫으면 대기 해제
    }

    void OnSignedIn(FirebaseUser user)
    {
        if (!waitingLogin)
            return;
        StopWaitingLogin();
        SettingCanvas.Instance.CloseCanvas();
        RankingCanvas.Instance.OpenCanvas();
    }

    void StopWaitingLogin()
    {
        waitingLogin = false;
        FirebaseAuth.OnSignedIn -= OnSignedIn;
    }

    void OnDestroy()
    {
        FirebaseAuth.OnSignedIn -= OnSignedIn;
    }
}
