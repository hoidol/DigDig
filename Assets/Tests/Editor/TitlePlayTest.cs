using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// (2026-11-27) 닉네임 규칙 (2~10자 · 금칙어 X · 기본값(GUID) X)
public class NicknameRuleTest
{
    [TestCase("슬라임", true)]
    [TestCase("  킹슬  ", true)] // 앞뒤 공백은 정리 (2자)
    [TestCase("King99", true)]
    [TestCase("킹 슬", false)]   // 가운데 공백 X
    [TestCase("킹!", false)]     // 특수문자 X
    [TestCase("ㅋㅋ", false)]     // 자모만 X
    [TestCase("킹_슬", false)]
    [TestCase("a", false)]       // 너무 짧음
    [TestCase("12345678901", false)] // 11자
    [TestCase("시발놈", false)]   // 금칙어
    [TestCase("", false)]
    public void Validate(string raw, bool ok)
    {
        Assert.AreEqual(ok, UserMyInfoManager.ValidateNickname(raw, out _) == null);
    }

    [TestCase("a.b")]
    [TestCase("a/b")]
    [TestCase("[킹]")]
    public void ServerKeyChars_NotAllowed(string raw) => Assert.IsNotNull(UserMyInfoManager.ValidateNickname(raw, out _));

    [Test]
    public void Guid_IsNotNickname()
    {
        Assert.IsNotNull(UserMyInfoManager.ValidateNickname(System.Guid.NewGuid().ToString(), out _), "기본값(GUID)은 닉네임이 아님");
    }
}

// (2026-11-27) Title 씬: (에디터 = 로그인 단계 건너뜀) 닉네임이 없으면 입력 창 → 잘못된 값은 오류 → 정하면 Lobby
public class TitlePlayTest : InGamePlayTestBase
{
    const string TITLE_SCENE = "Assets/4.Scenes/Title.unity";

    [UnityTest]
    public IEnumerator Title_Nickname_ThenLobby()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(TITLE_SCENE); yield return new EnterPlayMode(); }
        yield return WaitUntilReady(() => UserDataManager.Instance != null && Object.FindFirstObjectByType<TitleManager>() != null, "Title이 안 열림");
        yield return Body();
        AssertNoErrors();
    }

    static IEnumerator WaitFor(System.Func<bool> cond, string msg, float seconds = 20f)
    {
        double end = EditorApplication.timeSinceStartup + seconds;
        while (!cond())
        {
            Assert.Less(EditorApplication.timeSinceStartup, end, msg);
            if (EditorApplication.isPaused)
                EditorApplication.isPaused = false;
            yield return null;
        }
    }

    IEnumerator Body()
    {
        var info = UserDataManager.Instance.userMyInfoManager.userMyInfoData;
        if (UserDataManager.Instance.userMyInfoManager.HasNickname)
        {
            // 저장 데이터에 닉네임이 있으면 기본값으로 돌려놓고 Title을 다시 (저장 파일은 테스트 뒤 복원)
            info.nickname = System.Guid.NewGuid().ToString();
            SceneManager.LoadScene("Title");
            yield return WaitFor(() => SceneManager.GetActiveScene().name == "Title" && Object.FindFirstObjectByType<TitleManager>() != null, "Title 다시 열기");
            yield return WaitFrames(2);
        }
        NicknamePanel panel = Object.FindFirstObjectByType<NicknamePanel>(FindObjectsInactive.Include);
        yield return WaitFor(() => panel.gameObject.activeInHierarchy, "닉네임 창이 안 열림");
        Assert.AreEqual("Title", SceneManager.GetActiveScene().name, "닉네임 전에 로비로 감");

        panel.inputField.text = "a";
        panel.Confirm();
        Assert.IsFalse(UserDataManager.Instance.userMyInfoManager.HasNickname, "짧은 닉네임이 저장됨");

        // 중복 닉네임 = 저장 안 됨 (서버 대신 메모리)
        NicknameRegistry.fakeTaken["중복닉"] = "someone";
        panel.inputField.text = "중복닉";
        panel.Confirm();
        Assert.IsFalse(UserDataManager.Instance.userMyInfoManager.HasNickname, "이미 쓰는 닉네임이 저장됨");
        Assert.AreEqual(string.Format(TranslateManager.GetText("Nickname_Taken"), 2, 10), panel.errorText.text, "중복 안내");

        panel.inputField.text = "테스트킹";
        panel.Confirm();
        Assert.AreEqual("테스트킹", info.nickname);
        yield return WaitFor(() => SceneManager.GetActiveScene().name == "Lobby", "닉네임을 정했는데 로비로 안 감");
    }
}
