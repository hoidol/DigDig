using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

// 로비 랭킹 창 (CLAUDE.md "랭킹"): 랭킹 버튼 → 창, 난이도 드롭다운, 순위 줄 표시, 내 순위(목록 안 / 100+ / 기록 없음), 닫기
// 에디터에는 Firebase가 없어서 서버 대신 가짜 랭킹(RankingManager.FetchForTest)을 넣고 화면만 확인한다
public class RankingPlayTest : InGamePlayTestBase
{
    const string MY_UID = "me00testuid";

    [TearDown]
    public void ResetTestMode()
    {
        RankingManager.FetchForTest = null;
        RankingManager.UidForTest = null;
    }

    static RankData Data(string uid, string nickname, int wave, long updatedAt) => new RankData { uid = uid, nickname = nickname, wave = wave, updatedAt = updatedAt };

    // Normal: 5명(내가 2위, 같은 wave는 먼저 달성한 사람이 위) / Hard: 120명(나는 목록 밖) / Hell: 아무도 없음
    static RankingResult FakeFetch(DifficultyType difficulty)
    {
        var datas = new List<RankData>();
        if (difficulty == DifficultyType.Normal)
        {
            datas.Add(Data("u_c", "Charlie", 30, 500));
            datas.Add(Data("u_d", "Delta", 12, 50));
            datas.Add(Data(MY_UID, "Me", 30, 100));
            datas.Add(Data("u_a", "Alpha", 77, 900));
            datas.Add(Data("u_e", "Echo", 3, 10));
        }
        else if (difficulty == DifficultyType.Hard)
        {
            for (int i = 0; i < 120; i++)
                datas.Add(Data($"h{i}", $"Hard{i}", 500 - i, i));
        }
        return RankingManager.BuildResult(difficulty, datas, MY_UID, Time.realtimeSinceStartup);
    }

    static void SetMyRecord(DifficultyType difficulty, int maxWaveIdx, int tryCount)
    {
        UserStage stage = UserDataManager.Instance.userStageManager.GetUserStage(difficulty.ToString());
        stage.maxWave = maxWaveIdx;
        stage.tryCount = tryCount;
    }

    static RankingPanel[] Rows(RankingCanvas canvas)
        => canvas.rankingPanelParent.GetComponentsInChildren<RankingPanel>(true).Where(p => p.gameObject.activeSelf).ToArray();

    [UnityTest]
    public IEnumerator Lobby_RankingCanvas()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return WaitLobbyStart();
        yield return Body();
    }

    IEnumerator Body()
    {
        RankingManager.UidForTest = MY_UID;
        RankingManager.FetchForTest = FakeFetch;
        SetMyRecord(DifficultyType.Normal, 29, 3); // wave 30
        SetMyRecord(DifficultyType.Hard, 9, 1);    // wave 10 (목록 밖)
        SetMyRecord(DifficultyType.Hell, 0, 0);    // 기록 없음
        UserDataManager.difficultyType = DifficultyType.Normal;

        RankingCanvas canvas = RankingCanvas.Instance;
        Assert.IsNotNull(canvas, "로비에 RankingCanvas 없음");
        if (canvas.gameObject.activeSelf) // 씬에서 켜 둔 채 작업 중일 수 있음 → 닫고 시작
        {
            Debug.LogWarning("[RankingPlayTest] Lobby 씬에 RankingCanvas가 켜진 채 저장돼 있음");
            canvas.CloseCanvas();
            yield return WaitFrames(2);
        }
        Assert.IsNotNull(canvas.rankingPanelPrefab, "rankingPanelPrefab 연결 안 됨");
        Assert.IsNotNull(canvas.rankingPanelParent, "rankingPanelParent 연결 안 됨");
        Assert.IsNotNull(canvas.myRankingPanel, "myRankingPanel 연결 안 됨");
        Assert.IsNotNull(canvas.difficultDropdown, "difficultDropdown 연결 안 됨");
        foreach (RankingPanel p in new[] { canvas.rankingPanelPrefab, canvas.myRankingPanel })
        {
            Assert.IsNotNull(p.numText, $"{p.name}: numText 연결 안 됨");
            Assert.IsNotNull(p.nicknameText, $"{p.name}: nicknameText 연결 안 됨");
            Assert.IsNotNull(p.waveText, $"{p.name}: waveText 연결 안 됨");
            Assert.AreEqual(3, new[] { p.numText, p.nicknameText, p.waveText }.Distinct().Count(), $"{p.name}: 순위·닉네임·wave가 같은 글씨에 연결됨");
            Assert.IsTrue(p.numText.transform.IsChildOf(p.transform) && p.waveText.transform.IsChildOf(p.transform) && p.nicknameText.transform.IsChildOf(p.transform),
                $"{p.name}: 글씨가 줄 밖의 오브젝트에 연결됨 (머리글을 덮어씀)");
        }

        // 1) 랭킹 버튼 → 창. 현재 선택 난이도(Normal)로 시작
        RankingButton openButton = Object.FindFirstObjectByType<RankingButton>(FindObjectsInactive.Include);
        Assert.IsNotNull(openButton, "로비에 RankingButton 없음");
        Assert.IsTrue(openButton.gameObject.activeInHierarchy, "RankingButton이 꺼져 있음");
        openButton.GetComponent<Button>().onClick.Invoke();
        yield return WaitFrames(3);
        Assert.IsTrue(canvas.gameObject.activeInHierarchy, "랭킹 버튼을 눌러도 창이 안 열림");
        Assert.AreEqual(DifficultyType.Normal, canvas.CurrentDifficulty);

        // 드롭다운: 난이도 순서 + 번역 이름 (잠긴 난이도도 표시)
        var dropdown = canvas.difficultDropdown;
        Assert.AreEqual(new[] { "Normal", "Hard", "Hell" }.Select(d => TranslateManager.GetText($"Difficulty.{d}")).ToArray(),
            dropdown.options.Select(o => o.text).ToArray(), "드롭다운 옵션");
        Assert.AreEqual(0, dropdown.value, "열 때 현재 난이도로 맞춤");
        Assert.AreEqual(TranslateManager.GetText("Difficulty.Normal"), dropdown.captionText.text, "드롭다운에 보이는 글씨");

        // 2) Normal: 5줄, wave 내림차순(같으면 먼저 달성), 내 줄만 강조, 아래 내 순위 = 2위
        RankingPanel[] rows = Rows(canvas);
        Assert.AreEqual(new[] { "Alpha", "Me", "Charlie", "Delta", "Echo" }, rows.Select(r => r.nicknameText.text).ToArray(), "순위 줄 순서");
        Assert.AreEqual(new[] { "1", "2", "3", "4", "5" }, rows.Select(r => r.numText.text).ToArray(), "순위 글씨");
        Assert.AreEqual(new[] { "77", "30", "30", "12", "3" }, rows.Select(r => r.waveText.text).ToArray(), "wave 글씨 (숫자만)");
        Assert.AreEqual(new[] { false, true, false, false, false }, rows.Select(r => r.myHighlight.activeSelf).ToArray(), "내 줄만 강조");
        Assert.AreEqual("2", canvas.myRankingPanel.numText.text, "내 순위");
        Assert.AreEqual("Me", canvas.myRankingPanel.nicknameText.text);
        Assert.AreEqual("30", canvas.myRankingPanel.waveText.text);
        Assert.IsFalse(canvas.emptyObject.activeSelf, "랭킹이 있는데 '랭킹 없음' 표시");
        Assert.IsFalse(canvas.loadingObject.activeSelf, "불러온 뒤에도 로딩 표시");
        Assert.IsTrue(canvas.rankingPanelParent.GetComponent<LayoutGroup>() != null, "목록 부모에 LayoutGroup 없음 (줄이 겹침)");

        // 3) 드롭다운으로 Hard: 상위 100줄만, 나는 목록 밖 → "100+" + 내 로컬 기록
        dropdown.value = 1;
        yield return WaitFrames(3);
        Assert.AreEqual(DifficultyType.Hard, canvas.CurrentDifficulty, "드롭다운을 바꿔도 난이도가 안 바뀜");
        rows = Rows(canvas);
        Assert.AreEqual(RankingManager.TOP_COUNT, rows.Length, "상위 100명만");
        Assert.AreEqual("Hard0", rows[0].nicknameText.text);
        Assert.AreEqual("500", rows[0].waveText.text);
        Assert.AreEqual("100", rows[99].numText.text);
        Assert.AreEqual("Hard99", rows[99].nicknameText.text);
        Assert.IsTrue(rows.All(r => !r.myHighlight.activeSelf), "내가 목록에 없는데 강조된 줄이 있음");
        Assert.AreEqual("100+", canvas.myRankingPanel.numText.text, "목록 밖 내 순위");
        Assert.AreEqual("10", canvas.myRankingPanel.waveText.text, "목록 밖이면 내 로컬 최고 wave");
        Assert.AreEqual(RankingManager.GetMyNickname(MY_UID), canvas.myRankingPanel.nicknameText.text);
        Assert.IsTrue(canvas.myRankingPanel.myHighlight.activeSelf);

        // 4) Hell: 아무도 없음 + 내 기록 없음 → '랭킹 없음', 내 순위 "-"
        dropdown.value = 2;
        yield return WaitFrames(3);
        Assert.AreEqual(DifficultyType.Hell, canvas.CurrentDifficulty);
        Assert.AreEqual(0, Rows(canvas).Length, "빈 랭킹인데 이전 줄이 남음");
        Assert.IsTrue(canvas.emptyObject.activeSelf, "'랭킹 없음' 표시가 안 보임");
        Assert.AreEqual("-", canvas.myRankingPanel.numText.text, "기록 없음 내 순위");
        Assert.AreEqual("-", canvas.myRankingPanel.waveText.text, "기록 없음 wave");

        // 5) 다시 Normal: 줄 재사용 (복사한 줄이 100개를 넘지 않음)
        dropdown.value = 0;
        yield return WaitFrames(3);
        Assert.AreEqual(5, Rows(canvas).Length);
        Assert.IsFalse(canvas.emptyObject.activeSelf);
        Assert.AreEqual(RankingManager.TOP_COUNT, canvas.rankingPanelParent.GetComponentsInChildren<RankingPanel>(true).Length, "줄을 재사용하지 않고 계속 복사함");
        Assert.AreEqual("2", canvas.myRankingPanel.numText.text);

        // 6) 닫기 버튼 → 닫힘. 다른 난이도를 고른 뒤 다시 열면 그 난이도로
        Button close = canvas.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name.Contains("Close"));
        Assert.IsNotNull(close, "랭킹 창에 닫기 버튼 없음");
        close.onClick.Invoke();
        yield return WaitFrames(2);
        Assert.IsFalse(canvas.gameObject.activeInHierarchy, "닫기 버튼을 눌러도 안 닫힘");

        UserDataManager.difficultyType = DifficultyType.Hard;
        openButton.GetComponent<Button>().onClick.Invoke();
        yield return WaitFrames(3);
        Assert.AreEqual(DifficultyType.Hard, canvas.CurrentDifficulty, "다시 열 때 현재 선택 난이도");
        Assert.AreEqual(1, dropdown.value);
        Assert.AreEqual(RankingManager.TOP_COUNT, Rows(canvas).Length);
        canvas.CloseCanvas();
        UserDataManager.difficultyType = DifficultyType.Normal;
        yield return WaitFrames(2);
        AssertNoErrors();
    }
}
