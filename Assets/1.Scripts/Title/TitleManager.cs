using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// (2026-11-27) Title 씬 (빌드 첫 씬): 로그인해야 Lobby로. 처음이면 닉네임도 정함
// 흐름: 서버 연결(Firebase 초기화) → 이미 로그인(저장된 세션)이면 바로 다음 / 아니면 로그인 창(게스트·Google·Apple, 지원 플랫폼만)
//   → 닉네임이 없으면(기본값 GUID) 닉네임 입력 창 → Lobby
// 에디터엔 Firebase 네이티브가 없어 로그인 단계를 건너뜀 (skipLoginInEditor). UI = Title 씬 (메뉴 Tools/Title/Create Title Scene)
public class TitleManager : MonoBehaviour
{
    public const string LOBBY_SCENE = "Lobby";

    public GameObject connectingObject;   // "서버에 연결 중…"
    public GameObject loginPanel;         // 로그인 버튼들
    public GameObject googleButton;       // 지원 플랫폼에서만 보임
    public GameObject appleButton;
    public NicknamePanel nicknamePanel;
    public bool skipLoginInEditor = true;

    bool signedIn;
    bool leaving;

    void Start()
    {
        SetActive(loginPanel, false);
        if (nicknamePanel != null)
            nicknamePanel.gameObject.SetActive(false);
        FirebaseAuth.OnSignedIn += OnSignedIn;
        RunAsync().Forget();
    }

    void OnDestroy() => FirebaseAuth.OnSignedIn -= OnSignedIn;

    void OnSignedIn(Firebase.Auth.FirebaseUser user) => signedIn = true;

    static void SetActive(GameObject go, bool active)
    {
        if (go != null)
            go.SetActive(active);
    }

    async UniTaskVoid RunAsync()
    {
        var token = this.GetCancellationTokenOnDestroy();
        SetActive(connectingObject, true);
        FadeCanvs.Instance?.FadeIn("", null);

        bool skipLogin = Application.isEditor && skipLoginInEditor;
        if (!skipLogin)
        {
            FirebaseAuth auth = await WaitAuthAsync();
            if (token.IsCancellationRequested)
                return;
            SetActive(connectingObject, false);
            if (auth == null)
            {
                // 서버에 연결 못 함 → 다시 시도 안내 (오프라인으로는 들어가지 않음)
                ToastCanvas.Toast(TranslateManager.GetText("Server_Connecting_Retry"));
                SetActive(connectingObject, true);
                await UniTask.Delay(3000, cancellationToken: token).SuppressCancellationThrow();
                if (!token.IsCancellationRequested)
                    RunAsync().Forget();
                return;
            }
            if (!auth.IsSignedIn)
            {
                SetActive(googleButton, auth.IsGoogleSupported);
                SetActive(appleButton, auth.IsAppleSupported);
                SetActive(loginPanel, true);
                if (await UniTask.WaitUntil(() => signedIn || auth.IsSignedIn, cancellationToken: token).SuppressCancellationThrow())
                    return;
                SetActive(loginPanel, false);
            }
            UserDataManager.Instance.SetUid(auth.UserId);
        }
        SetActive(connectingObject, false);

        // 닉네임
        UserMyInfoManager myInfo = UserDataManager.Instance.userMyInfoManager;
        if (!myInfo.HasNickname)
        {
            if (nicknamePanel == null)
            {
                Debug.LogWarning("[Title] nicknamePanel 없음 — 메뉴 Tools/Title/Create Title Scene");
            }
            else
            {
                nicknamePanel.Open();
                if (await UniTask.WaitUntil(() => myInfo.HasNickname, cancellationToken: token).SuppressCancellationThrow())
                    return;
                nicknamePanel.gameObject.SetActive(false);
            }
        }
        GoLobby();
    }

    // Firebase 초기화 + Auth 준비 대기 (실패하면 null)
    static async UniTask<FirebaseAuth> WaitAuthAsync()
    {
        if (FirebaseManager.Instance == null)
            return null;
        if (!await FirebaseManager.Instance.WaitInitAsync())
            return null;
        FirebaseAuth auth = FirebaseManager.Instance.firebaseAuth;
        return auth != null && auth.IsReady ? auth : null;
    }

    void GoLobby()
    {
        if (leaving)
            return;
        leaving = true;
        if (FadeCanvs.Instance != null)
            FadeCanvs.Instance.FadeOutIn(null, () => SceneManager.LoadScene(LOBBY_SCENE));
        else
            SceneManager.LoadScene(LOBBY_SCENE);
    }
}
