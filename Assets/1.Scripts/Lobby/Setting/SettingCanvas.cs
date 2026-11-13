using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;

// 설정 창
// - BGM / SFX 스위치 : 끄면 음소거 (AudioVolumeUtil, 볼륨 값과 별개로 저장)
// - 진동 스위치 : Vibration
// - 언어 버튼 : 누를 때마다 다음 언어 (번역 데이터가 있는 언어만)
// - 로그인 안 됨 = LoginPanel(게스트 / Google / Apple), 로그인됨 = LoginStatePanel(Discord / 서버 저장 / 불러오기 / Player ID)
//   Google / Apple 버튼은 지원하는 플랫폼에서만 보인다.
// 로그인·저장·불러오기 버튼의 동작은 각 버튼 컴포넌트(FirebaseXxxLoginButton, UserDataSaveButton, UserDataLoadButton)
public class SettingCanvas : CanvasUI<SettingCanvas>
{
    // 번역 시트에 있는 언어 (누르면 이 순서로 순환)
    public static readonly LanguageType[] LANGUAGES = { LanguageType.kr, LanguageType.en };
    public const string DISCORD_URL = ""; // TODO 디스코드 초대 주소. 비어 있으면 준비 중 토스트
    const string UID_FORMAT = "<color=#BEB2AD>Player ID</color> {0}";

    [Header("소리 / 진동")]
    public SettingSwitch bgmSwitch;
    public SettingSwitch sfxSwitch;
    public SettingSwitch hapticSwitch;

    [Header("언어")]
    public Button languageButton;
    public TMP_Text languageText;

    [Header("로그인")]
    public GameObject loginPanel; // 로그인 안 됨
    public GameObject loginStatePanel; // 로그인됨
    public GameObject googleLoginButton;
    public GameObject appleLoginButton;

    [Header("로그인 상태")]
    public Button discordButton;
    public Button uidCopyButton;
    public TMP_Text uidText;

    [Header("기타")]
    public Button closeButton;
    public TMP_Text versionText;

    void Awake()
    {
        if (bgmSwitch != null)
            bgmSwitch.onChanged = on => SetMute(AudioVolumeUtil.BGMMuteKey, !on);
        if (sfxSwitch != null)
            sfxSwitch.onChanged = on => SetMute(AudioVolumeUtil.SFXMuteKey, !on);
        if (hapticSwitch != null)
            hapticSwitch.onChanged = OnHapticChanged;

        if (languageButton != null)
            languageButton.onClick.AddListener(OnClickedLanguage);
        if (discordButton != null)
            discordButton.onClick.AddListener(OnClickedDiscord);
        if (uidCopyButton != null)
            uidCopyButton.onClick.AddListener(OnClickedCopyUid);
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseCanvas);
    }

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
        if (bgmSwitch != null)
            bgmSwitch.SetOn(!AudioVolumeUtil.IsMuted(AudioVolumeUtil.BGMMuteKey));
        if (sfxSwitch != null)
            sfxSwitch.SetOn(!AudioVolumeUtil.IsMuted(AudioVolumeUtil.SFXMuteKey));
        if (hapticSwitch != null)
            hapticSwitch.SetOn(Vibration.IsVibrationEnabled());

        if (languageText != null)
            languageText.text = TranslateManager.GetTitle(TranslateManager.GetLanguageType());
        if (versionText != null)
            versionText.text = $"Version {Application.version}";

        RefreshLogin();
    }

    // ---------- 소리 / 진동 ----------
    static void SetMute(string muteKey, bool mute)
    {
        AudioVolumeUtil.SetMuted(muteKey, mute);
        if (SoundManager.Instance != null)
            SoundManager.Instance.ApplyVolumes();
    }

    static void OnHapticChanged(bool on)
    {
        Vibration.SetVibrationEnabled(on);
        if (on)
            Vibration.Vibrate(0); // 켰을 때 한 번 울려서 확인
    }

    // ---------- 언어 ----------
    public static LanguageType GetNextLanguage(LanguageType current)
    {
        int idx = System.Array.IndexOf(LANGUAGES, current);
        return LANGUAGES[(idx + 1) % LANGUAGES.Length]; // 목록에 없는 언어(-1)면 첫 언어
    }

    void OnClickedLanguage()
    {
        TranslateManager.ChangeLanguage(GetNextLanguage(TranslateManager.GetLanguageType()));
        Refresh();
    }

    // ---------- 로그인 ----------
    void RefreshLogin()
    {
        FirebaseAuth auth = FirebaseManager.Instance != null ? FirebaseManager.Instance.firebaseAuth : null;
        bool ready = auth != null && auth.IsReady;
        bool signedIn = ready && auth.IsSignedIn;

        SetActive(loginPanel, !signedIn);
        SetActive(loginStatePanel, signedIn);
        SetActive(googleLoginButton, ready && auth.IsGoogleSupported);
        SetActive(appleLoginButton, ready && auth.IsAppleSupported);

        if (uidText != null)
            uidText.text = string.Format(UID_FORMAT, signedIn ? auth.UserId : "");
    }

    void OnClickedDiscord()
    {
        if (string.IsNullOrEmpty(DISCORD_URL))
        {
            ToastCanvas.Toast(TranslateManager.GetText("Setting_ComingSoon"));
            return;
        }
        Application.OpenURL(DISCORD_URL);
    }

    // Player ID를 누르면 복사 (문의할 때 전달용)
    void OnClickedCopyUid()
    {
        FirebaseAuth auth = FirebaseManager.Instance != null ? FirebaseManager.Instance.firebaseAuth : null;
        if (auth == null || !auth.IsReady || !auth.IsSignedIn)
            return;
        GUIUtility.systemCopyBuffer = auth.UserId;
        ToastCanvas.Toast(TranslateManager.GetText("Setting_UidCopied"));
    }

    static void SetActive(GameObject go, bool active)
    {
        if (go != null && go.activeSelf != active)
            go.SetActive(active);
    }
}
