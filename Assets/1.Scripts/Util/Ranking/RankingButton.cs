using Firebase.Auth;

// 로그인돼 있으면 바로 RankingCanvas, 아니면 FirebaseLoginCanvas → 로그인 성공 시 RankingCanvas
public class RankingButton : ButtonUI
{
    bool waitingLogin;

    public override void OnClickedBtn()
    {
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
        FirebaseLoginCanvas.Instance.OpenCanvas(StopWaitingLogin); // 로그인 없이 닫으면 대기 해제
    }

    void OnSignedIn(FirebaseUser user)
    {
        if (!waitingLogin)
            return;
        StopWaitingLogin();
        FirebaseLoginCanvas.Instance.CloseCanvas();
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
