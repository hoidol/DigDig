using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

// 로비 설정 창 (CLAUDE.md "설정 창"): 설정 버튼 → 창, BGM·SFX 음소거 / 진동 스위치, 언어 순환, 로그인 상태별 패널, 닫기
public class SettingPlayTest : InGamePlayTestBase
{
    [Test]
    public void NextLanguage_CyclesTranslatedLanguages()
    {
        Assert.AreEqual(LanguageType.en, SettingCanvas.GetNextLanguage(LanguageType.kr));
        Assert.AreEqual(LanguageType.kr, SettingCanvas.GetNextLanguage(LanguageType.en));
        Assert.AreEqual(LanguageType.kr, SettingCanvas.GetNextLanguage(LanguageType.vi), "번역 없는 언어 → 첫 언어");
    }

    [UnityTest]
    public IEnumerator Lobby_SettingCanvas()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return WaitLobbyStart();
        yield return Lobby_SettingCanvasBody();
    }

    IEnumerator Lobby_SettingCanvasBody()
    {
        // 테스트가 바꾸는 설정은 끝나고 되돌린다
        bool bgmMuted = AudioVolumeUtil.IsMuted(AudioVolumeUtil.BGMMuteKey);
        bool sfxMuted = AudioVolumeUtil.IsMuted(AudioVolumeUtil.SFXMuteKey);
        bool haptic = Vibration.IsVibrationEnabled();
        LanguageType language = TranslateManager.GetLanguageType();

        SettingCanvas canvas = SettingCanvas.Instance;
        Assert.IsNotNull(canvas, "로비에 SettingCanvas 없음");
        Assert.AreEqual(1, Object.FindObjectsByType<SettingCanvas>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length, "SettingCanvas는 하나");
        Assert.IsFalse(canvas.gameObject.activeSelf, "로비 시작 시 설정 창이 열려 있음");

        // 설정 버튼 → 창
        SettingButton settingButton = Object.FindFirstObjectByType<SettingButton>(FindObjectsInactive.Include);
        Assert.IsNotNull(settingButton, "로비에 설정 버튼 없음");
        settingButton.GetComponent<Button>().onClick.Invoke();
        yield return WaitFrames(2);
        Assert.IsTrue(canvas.gameObject.activeInHierarchy, "설정 버튼을 눌러도 창이 안 열림");

        // BGM / SFX 스위치 = 음소거 (저장값 + 적용 볼륨 0)
        yield return CheckMuteSwitch(canvas.bgmSwitch, AudioVolumeUtil.BGMMuteKey, AudioVolumeUtil.BGMVolumeKey, "BGM");
        yield return CheckMuteSwitch(canvas.sfxSwitch, AudioVolumeUtil.SFXMuteKey, AudioVolumeUtil.SFXVolumeKey, "SFX");

        // 진동 스위치
        Assert.AreEqual(haptic, canvas.hapticSwitch.IsOn, "진동 스위치 처음 표시");
        Click(canvas.hapticSwitch);
        Assert.AreEqual(!haptic, Vibration.IsVibrationEnabled(), "진동 스위치를 눌러도 설정이 안 바뀜");
        Click(canvas.hapticSwitch);
        Assert.AreEqual(haptic, Vibration.IsVibrationEnabled(), "진동 스위치 되돌리기");

        // 언어 : 누를 때마다 다음 언어, 버튼 글씨 = 현재 언어 이름
        Assert.AreEqual(TranslateManager.GetTitle(language), canvas.languageText.text, "언어 버튼 처음 표시");
        canvas.languageButton.onClick.Invoke();
        yield return WaitFrames(2);
        LanguageType next = SettingCanvas.GetNextLanguage(language);
        Assert.AreEqual(next, TranslateManager.GetLanguageType(), "언어 버튼을 눌러도 언어가 안 바뀜");
        Assert.AreEqual(TranslateManager.GetTitle(next), canvas.languageText.text, "언어 버튼 글씨");
        Assert.AreEqual((int)next, PlayerPrefs.GetInt("LanguageType", -1), "언어 저장");

        // 씬에 박힌 고정 글씨(TranslateText)도 바뀐 언어로 (꺼져 있는 창 포함)
        TranslateText[] fixedTexts = Object.FindObjectsByType<TranslateText>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Assert.Greater(fixedTexts.Length, 50, "로비 고정 글씨에 TranslateText가 없음");
        foreach (TranslateText fixedText in fixedTexts)
        {
            if (string.IsNullOrEmpty(fixedText.key))
                continue;
            Assert.IsTrue(TranslateManager.HasKey(fixedText.key), $"번역 시트에 없는 key: {fixedText.key} ({fixedText.name})");
            string expected = TranslateManager.GetText(fixedText.key);
            if (!string.IsNullOrEmpty(fixedText.formatArg))
                expected = string.Format(expected, fixedText.formatArg);
            Assert.AreEqual(expected, fixedText.GetComponent<TMPro.TMP_Text>().text, $"언어 변경 후 고정 글씨: {fixedText.key}");
        }
        TranslateManager.ChangeLanguage(language);

        // 로그인 상태에 맞는 패널 하나만 (에디터에선 Firebase 상태가 환경마다 달라 실제 상태 기준으로 확인)
        FirebaseAuth auth = FirebaseManager.Instance != null ? FirebaseManager.Instance.firebaseAuth : null;
        bool signedIn = auth != null && auth.IsReady && auth.IsSignedIn;
        canvas.Refresh();
        Assert.AreEqual(!signedIn, canvas.loginPanel.activeSelf, $"로그인 패널 (로그인됨 = {signedIn})");
        Assert.AreEqual(signedIn, canvas.loginStatePanel.activeSelf, $"로그인 상태 패널 (로그인됨 = {signedIn})");
        if (signedIn)
            StringAssert.Contains(auth.UserId, canvas.uidText.text, "Player ID 표시");
        Debug.Log($"[SettingPlayTest] signedIn = {signedIn}");

        // 버튼 연결 (컴포넌트가 붙어 있어야 동작)
        Assert.IsNotNull(canvas.loginPanel.GetComponentInChildren<FirebaseAnonyLoginButton>(true), "게스트 로그인 버튼");
        Assert.IsNotNull(canvas.googleLoginButton.GetComponent<FirebaseGoogleLoginButton>(), "Google 로그인 버튼");
        Assert.IsNotNull(canvas.appleLoginButton.GetComponent<FirebaseAppleLoginButton>(), "Apple 로그인 버튼");
        Assert.IsNotNull(canvas.loginStatePanel.GetComponentInChildren<UserDataSaveButton>(true), "서버 저장 버튼");
        Assert.IsNotNull(canvas.loginStatePanel.GetComponentInChildren<UserDataLoadButton>(true), "서버 불러오기 버튼");
        Assert.IsNotNull(canvas.discordButton, "Discord 버튼");
        Assert.IsNotNull(canvas.uidCopyButton, "Player ID 복사 버튼");
        Assert.IsNotNull(canvas.uidText, "Player ID 글씨");
        Assert.AreEqual($"Version {Application.version}", canvas.versionText.text, "버전 표시");

        // Discord 주소가 아직 없으면 눌러도 에러 없이 토스트만
        canvas.discordButton.onClick.Invoke();
        yield return WaitFrames(2);

        // 닫기
        canvas.closeButton.onClick.Invoke();
        yield return WaitFrames(2);
        Assert.IsFalse(canvas.gameObject.activeSelf, "닫기 버튼을 눌러도 창이 안 닫힘");

        AudioVolumeUtil.SetMuted(AudioVolumeUtil.BGMMuteKey, bgmMuted);
        AudioVolumeUtil.SetMuted(AudioVolumeUtil.SFXMuteKey, sfxMuted);
        SoundManager.Instance.ApplyVolumes();
        Vibration.SetVibrationEnabled(haptic);
        AssertNoErrors("설정 창 에러");
    }

    static void Click(SettingSwitch sw) => sw.GetComponent<Button>().onClick.Invoke();

    IEnumerator CheckMuteSwitch(SettingSwitch sw, string muteKey, string volumeKey, string name)
    {
        Assert.IsNotNull(sw, $"{name} 스위치 연결 안 됨");
        bool muted = AudioVolumeUtil.IsMuted(muteKey);
        Assert.AreEqual(!muted, sw.IsOn, $"{name} 스위치 처음 표시 (켜짐 = 소리 남)");
        Assert.AreNotEqual(sw.onObject.activeSelf, sw.offObject.activeSelf, $"{name} On/Off 중 하나만 보여야 함");

        if (muted) // 소리 나는 상태에서 시작
        {
            Click(sw);
            yield return WaitFrames(1);
        }
        Assert.IsFalse(AudioVolumeUtil.IsMuted(muteKey), $"{name} 켜짐");
        Assert.AreEqual(PlayerPrefs.GetFloat(volumeKey, 1f), AudioVolumeUtil.GetAppliedVolume(volumeKey, muteKey), $"{name} 켜짐: 저장된 볼륨 그대로");

        Click(sw);
        yield return WaitFrames(1);
        Assert.IsTrue(AudioVolumeUtil.IsMuted(muteKey), $"{name} 스위치를 꺼도 음소거가 안 됨");
        Assert.AreEqual(0f, AudioVolumeUtil.GetAppliedVolume(volumeKey, muteKey), $"{name} 음소거: 적용 볼륨 0");
        Assert.IsFalse(sw.onObject.activeSelf, $"{name} 꺼짐 표시");
        Assert.IsTrue(sw.offObject.activeSelf, $"{name} 꺼짐 표시");
    }
}
