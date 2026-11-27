using TMPro;
using UnityEngine;

// (2026-11-24) KS 관리 화면 레벨 스탯: 묶음 N줄 × 스탯 칸 (줄 = 레벨 구간 Lv.1~5·6~10…, 그 구간에서 얻은 포인트만 그 줄에, SlimeLevelStatSetting)
// 이전 권능 목록(AuthorityContainer → 2026-11-27 씬 이름 TrainingContainer)을 바꿈. 씬 연결 유지 위해 guid 그대로 → slotPanels(18칸) 연결도 그대로
// 칸 순서 = 묶음 순서대로 묶음마다 SLOTS_PER_TIER칸. 씬에서 직접 만든 UI를 연결, 비운 칸은 건너뜀
public class LevelStatContainer : KSMContainer
{
    public const int SLOTS_PER_TIER = 3;

    public LevelStatSlotPanel[] slotPanels;
    [Tooltip("남은 포인트 \"남은 포인트 N\" (번역 SlimeStat_RemainPoint)")]
    public TMP_Text remainPointText;
    [Tooltip("묶음마다 찍은 포인트 / 그 묶음에서 받은 포인트 \"2/3\" (묶음 순서)")]
    public TMP_Text[] tierPointTexts;
    [Tooltip("(2026-11-27) 묶음마다 남은 포인트 \"남은 포인트 N\" = 그 묶음에서 받은 포인트 - 찍은 포인트 (묶음 제목 LvBg 오른쪽, 번역 SlimeStat_TierRemain). 받은 포인트가 없는 묶음은 숨김")]
    public TMP_Text[] tierRemainTexts;
    public Color remainColor = new Color(1f, 0.85f, 0.25f);      // 남은 포인트가 있을 때
    public Color noRemainColor = new Color(0.75f, 0.75f, 0.8f);  // 다 찍었을 때

    public override void UpdateContainer()
    {
        var user = UserDataManager.Instance.userSlimeManager;
        var tiers = SlimeLevelStatSetting.Tiers;
        for (int i = 0; i < slotPanels.Length; i++)
        {
            int tierIdx = i / SLOTS_PER_TIER;
            int optionIdx = i % SLOTS_PER_TIER;
            if (slotPanels[i] != null)
                slotPanels[i].SetOption(tierIdx, optionIdx, tierIdx < tiers.Length ? tiers[tierIdx] : null);
        }

        if (remainPointText != null)
            remainPointText.text = string.Format(TranslateManager.GetText("SlimeStat_RemainPoint"), user.RemainPoints);
        if (tierPointTexts != null)
        {
            for (int i = 0; i < tierPointTexts.Length; i++)
            {
                if (tierPointTexts[i] == null)
                    continue;
                bool has = i < tiers.Length;
                tierPointTexts[i].gameObject.SetActive(has);
                if (has)
                    tierPointTexts[i].text = $"{user.GetTierPoints(i)}/{user.GetTierEarnedPoints(i)}";
            }
        }
        UpdateTierRemainTexts(user, tiers.Length);
    }

    void UpdateTierRemainTexts(UserSlimeManager user, int tierCount)
    {
        if (tierRemainTexts == null)
            return;
        for (int i = 0; i < tierRemainTexts.Length; i++)
        {
            if (tierRemainTexts[i] == null)
                continue;
            int earned = i < tierCount ? user.GetTierEarnedPoints(i) : 0;
            tierRemainTexts[i].gameObject.SetActive(earned > 0);
            if (earned <= 0)
                continue;
            int remain = Mathf.Max(0, earned - user.GetTierPoints(i));
            tierRemainTexts[i].text = string.Format(TranslateManager.GetText("SlimeStat_TierRemain"), remain);
            tierRemainTexts[i].color = remain > 0 ? remainColor : noRemainColor;
        }
    }

    // 초기화 버튼 onClick (씬에서 연결): 찍은 포인트 전부 돌려받음 (무료)
    public void OnClickedReset()
    {
        var user = UserDataManager.Instance.userSlimeManager;
        if (user.UsedPoints <= 0)
            return;
        YesOrNoCanvas.Instance.OpenCanvas(TranslateManager.GetText("SlimeStat_ResetTitle"), TranslateManager.GetText("SlimeStat_ResetBody"), yes =>
        {
            if (!yes)
                return;
            user.ResetPoints();
            ToastCanvas.Toast(TranslateManager.GetText("SlimeStat_ResetDone"));
            Lobby.LobbyManager.Instance.GetLobbyCanvas(LobbyState.SlimeManagement).UpdateCanvas();
        });
    }
}
