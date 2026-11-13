using UnityEngine;

// 누적 업적 칸. 받으면 다음 목표로 넘어가므로 상태는 둘뿐: 받을 수 있음 → claimObj / 진행 중 → processingObj
public class CumulativeAchievementPanel : AchievementPanel
{
    public GameObject processingObj;

    CumulativeAchievementData CumulativeData => achievementData as CumulativeAchievementData;

    public override void UpdatePanel()
    {
        base.UpdatePanel();
        if (CumulativeData == null)
            return;

        bool canClaim = CumulativeData.CheckCanClear();
        SetStateObj(claimObj, canClaim);
        SetStateObj(processingObj, !canClaim);
        SetStateObj(redDot, canClaim);
    }

    public override void OnClickedGetReward()
    {
        if (CumulativeData == null)
            return;
        AchievementManager.Instance.cumulativeAchievementManager.GetReward(CumulativeData); // 받을 수 없으면 안에서 무시
        AchievementCanvas.Instance.UpdateCanvas();
    }

}
