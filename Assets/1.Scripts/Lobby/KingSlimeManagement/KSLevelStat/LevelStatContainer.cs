using TMPro;
using UnityEngine;

// (2026-11-24) KS 관리 화면 레벨 스탯: 묶음 N줄 × 스탯 칸 (줄 = 해금 레벨 5·10·15…, KingLevelStatSetting)
// 이전 권능 목록(AuthorityContainer)을 바꿈. 씬 연결 유지 위해 guid 그대로 → slotPanels(18칸) 연결도 그대로
// 칸 순서 = 묶음 순서대로 묶음마다 SLOTS_PER_TIER칸. 씬에서 직접 만든 UI를 연결, 비운 칸은 건너뜀
public class LevelStatContainer : KSMContainer
{
    public const int SLOTS_PER_TIER = 3;

    public LevelStatSlotPanel[] slotPanels;
    [Tooltip("남은 포인트 \"남은 포인트 N\" (번역 KingStat_RemainPoint)")]
    public TMP_Text remainPointText;
    [Tooltip("묶음마다 찍은 포인트 \"2/5\" (묶음 순서)")]
    public TMP_Text[] tierPointTexts;

    public override void UpdateContainer()
    {
        var user = UserDataManager.Instance.userKingSlimeManager;
        var tiers = KingLevelStatSetting.Tiers;
        for (int i = 0; i < slotPanels.Length; i++)
        {
            int tierIdx = i / SLOTS_PER_TIER;
            int optionIdx = i % SLOTS_PER_TIER;
            if (slotPanels[i] != null)
                slotPanels[i].SetOption(tierIdx, optionIdx, tierIdx < tiers.Length ? tiers[tierIdx] : null);
        }

        if (remainPointText != null)
            remainPointText.text = string.Format(TranslateManager.GetText("KingStat_RemainPoint"), user.RemainPoints);
        if (tierPointTexts != null)
        {
            for (int i = 0; i < tierPointTexts.Length; i++)
            {
                if (tierPointTexts[i] == null)
                    continue;
                bool has = i < tiers.Length;
                tierPointTexts[i].gameObject.SetActive(has);
                if (has)
                    tierPointTexts[i].text = $"{user.GetTierPoints(i)}/{tiers[i].maxPoints}";
            }
        }
    }

    // 초기화 버튼 onClick (씬에서 연결): 찍은 포인트 전부 돌려받음 (무료)
    public void OnClickedReset()
    {
        var user = UserDataManager.Instance.userKingSlimeManager;
        if (user.UsedPoints <= 0)
            return;
        YesOrNoCanvas.Instance.OpenCanvas(TranslateManager.GetText("KingStat_ResetTitle"), TranslateManager.GetText("KingStat_ResetBody"), yes =>
        {
            if (!yes)
                return;
            user.ResetPoints();
            ToastCanvas.Toast(TranslateManager.GetText("KingStat_ResetDone"));
            Lobby.LobbyManager.Instance.GetLobbyCanvas(LobbyState.KingSlimeManagement).UpdateCanvas();
        });
    }
}
