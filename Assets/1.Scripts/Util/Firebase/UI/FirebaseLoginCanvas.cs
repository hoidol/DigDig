using UnityEngine;
using TMPro;
using Firebase.Auth;

// 로그인 상태 표시 + 상태에 맞는 버튼만 노출
// - 로그인 안 됨 : 게스트 / Google / Apple
// - 게스트      : Google / Apple(계정 연결) / 로그아웃
// - Google/Apple: 로그아웃
// Google/Apple 버튼은 지원하는 플랫폼에서만 보인다.
public class FirebaseLoginCanvas : CanvasUI<FirebaseLoginCanvas>
{
    public TMP_Text statusText;
    public GameObject anonyLoginButton;
    public GameObject googleLoginButton;
    public GameObject appleLoginButton;
    public GameObject logoutButton;

    void OnEnable()
    {
        FirebaseAuth.OnSignedIn += OnSignedIn;
        FirebaseAuth.OnSignedOut += Refresh;
        FirebaseManager.OnInitialized += OnFirebaseInitialized;
        Refresh();
    }

    void OnDisable()
    {
        FirebaseAuth.OnSignedIn -= OnSignedIn;
        FirebaseAuth.OnSignedOut -= Refresh;
        FirebaseManager.OnInitialized -= OnFirebaseInitialized;
    }

    void OnSignedIn(FirebaseUser user) => Refresh();
    void OnFirebaseInitialized(bool success) => Refresh();

    public void Refresh()
    {
        FirebaseAuth auth = FirebaseManager.Instance != null ? FirebaseManager.Instance.firebaseAuth : null;
        bool ready = auth != null && auth.IsReady;
        LoginType type = ready ? auth.CurrentLoginType : LoginType.None;
        bool signedIn = ready && auth.IsSignedIn;
        bool canLinkAccount = ready && (type == LoginType.None || type == LoginType.Anonymous);

        if (statusText != null)
            statusText.text = GetStatusText(auth, ready, type);

        SetActive(anonyLoginButton, ready && !signedIn);
        SetActive(googleLoginButton, canLinkAccount && auth.IsGoogleSupported);
        SetActive(appleLoginButton, canLinkAccount && auth.IsAppleSupported);
        SetActive(logoutButton, signedIn);
    }

    static string GetStatusText(FirebaseAuth auth, bool ready, LoginType type)
    {
        if (!ready)
            return "서버 연결 중...";

        switch (type)
        {
            case LoginType.Anonymous: return $"게스트 ({ShortId(auth.UserId)})";
            case LoginType.Google: return "Google 계정 연결됨";
            case LoginType.Apple: return "Apple 계정 연결됨";
            default: return auth.IsSignedIn ? "로그인됨" : "로그인 필요";
        }
    }

    static string ShortId(string uid) => string.IsNullOrEmpty(uid) || uid.Length <= 8 ? uid : uid.Substring(0, 8);

    static void SetActive(GameObject go, bool active)
    {
        if (go != null && go.activeSelf != active)
            go.SetActive(active);
    }
}
