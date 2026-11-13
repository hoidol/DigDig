using UnityEngine;

// 일일 업적 칸. 상태 셋 중 하나만 보임: 보상 받음 → gottenRewardObj / 받을 수 있음 → claimObj / 그 외(진행 중) → processingObj
public class DailyAchievementPanel : AchievementPanel
{
    public GameObject gottenRewardObj;
    public GameObject processingObj;

    DailyAchievementData DailyData => achievementData as DailyAchievementData;

    public override void UpdatePanel()
    {
        base.UpdatePanel();
        if (DailyData == null)
        {
            return;
        }

        // 날짜가 바뀌면 유저 기록이 새로 만들어지므로 매번 다시 가져온다
        UserDailyAchievement userDailyAchievement = DailyData.GetUserAchievement() as UserDailyAchievement;
        bool gotten = userDailyAchievement.getReward;
        bool canClaim = !gotten && DailyData.CheckCanClear();
        SetStateObj(gottenRewardObj, gotten);
        SetStateObj(claimObj, canClaim);
        SetStateObj(processingObj, !gotten && !canClaim);
        SetStateObj(redDot, canClaim);
    }

    public override void OnClickedGetReward()
    {
        if (DailyData == null)
            return;
        AchievementManager.Instance.dailyAchievementManager.GetReward(DailyData); // 받을 수 없으면 안에서 무시
        AchievementCanvas.Instance.UpdateCanvas();
    }

}
