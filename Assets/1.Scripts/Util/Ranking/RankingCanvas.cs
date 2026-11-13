using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;

// 난이도별 랭킹 표시. 열 때마다 RankingManager에 요청하지만 목록은 10분 캐시라 실제 다운로드는 그 주기로만 일어난다.
public class RankingCanvas : CanvasUI<RankingCanvas>
{
    public RankingPanel rankingPanelPrefab;
    public Transform rankingPanelParent;
    public RankingPanel myRankingPanel;    // 내 순위 (목록 밖이면 "100+")
    public GameObject loadingObject;        // 불러오는 중 표시 (선택)
    public GameObject emptyObject;          // 랭킹 없음/실패 표시 (선택)
    public TMP_Text refreshTimeText;        // 다음 갱신까지 남은 시간 (선택)

    public TMP_Dropdown difficultDropdown; // 난이도 선택 (옵션은 코드가 DifficultyType 순서 + 번역 이름으로 채움. 잠긴 난이도도 볼 수 있음)

    readonly List<RankingPanel> panels = new List<RankingPanel>();
    DifficultyType currentDifficulty;
    int requestId; // 늦게 도착한 이전 요청 결과 무시용

    static readonly DifficultyType[] DIFFICULTIES = (DifficultyType[])Enum.GetValues(typeof(DifficultyType));
    bool dropdownReady;

    public DifficultyType CurrentDifficulty => currentDifficulty;

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        SetupDropdown();
        SelectDifficulty(UserDataManager.difficultyType);
    }

    void SetupDropdown()
    {
        if (difficultDropdown == null)
            return;
        // 언어가 바뀔 수 있으니 열 때마다 이름을 다시 채움
        difficultDropdown.ClearOptions();
        List<string> names = new List<string>();
        foreach (DifficultyType difficulty in DIFFICULTIES)
            names.Add(TranslateManager.GetText($"Difficulty.{difficulty}"));
        difficultDropdown.AddOptions(names);
        if (!dropdownReady)
        {
            dropdownReady = true;
            difficultDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }
    }

    void OnDropdownChanged(int index)
    {
        if (index < 0 || index >= DIFFICULTIES.Length)
            return;
        SelectDifficulty(DIFFICULTIES[index]);
    }

    public void SelectDifficulty(DifficultyType difficulty)
    {
        currentDifficulty = difficulty;
        if (difficultDropdown != null)
            difficultDropdown.SetValueWithoutNotify(Array.IndexOf(DIFFICULTIES, difficulty));

        LoadAsync(difficulty).Forget();
    }

    async UniTaskVoid LoadAsync(DifficultyType difficulty)
    {
        int id = ++requestId;
        SetActive(loadingObject, true);
        SetActive(emptyObject, false);

        RankingResult result = await RankingManager.GetRankingAsync(difficulty);
        if (this == null || id != requestId || !gameObject.activeInHierarchy)
            return;

        SetActive(loadingObject, false);
        Render(result);
    }

    void Render(RankingResult result)
    {
        List<RankData> ranks = result?.ranks;
        int count = ranks?.Count ?? 0;

        while (panels.Count < count)
            panels.Add(Instantiate(rankingPanelPrefab, rankingPanelParent));
        for (int i = 0; i < panels.Count; i++)
        {
            bool active = i < count;
            panels[i].gameObject.SetActive(active);
            if (active)
                panels[i].SetRanking(ranks[i], ranks[i] == result.my);
        }

        SetActive(emptyObject, count == 0);
        RenderMyRank(result);
    }

    void RenderMyRank(RankingResult result)
    {
        if (myRankingPanel == null)
            return;

        if (result?.my != null)
        {
            myRankingPanel.SetRanking(result.my, true);
            return;
        }

        // 목록 밖이거나 기록 없음 → 로컬 기록으로 표시
        int myWave = RankingManager.GetMyWave(currentDifficulty);
        string uid = RankingManager.MyUid;
        myRankingPanel.SetRanking(new RankData
        {
            num = 0,
            uid = uid,
            nickname = RankingManager.GetMyNickname(uid),
            wave = myWave,
        }, true, RankingManager.GetOutOfRankLabel(myWave));
    }

    void Update()
    {
        if (refreshTimeText == null)
            return;
        int sec = Mathf.CeilToInt(RankingManager.GetRemainRefreshSec(currentDifficulty));
        refreshTimeText.text = sec > 0 ? string.Format(TranslateManager.GetText("Ranking_RefreshIn"), sec / 60, sec % 60) : "";
    }

    static void SetActive(GameObject go, bool active)
    {
        if (go != null && go.activeSelf != active)
            go.SetActive(active);
    }
}
