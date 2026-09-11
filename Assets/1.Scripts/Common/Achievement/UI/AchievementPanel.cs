using TMPro;
using UnityEngine;

public abstract class AchievementPanel : MonoBehaviour
{
    public TMP_Text titleText; //스톤 부수기
    public TMP_Text descText; //부순 횟수 {0}/{goal}
    
    
    AchievementData achievementData;
    public virtual void SetAchievementData(AchievementData aData)
    {
        achievementData = aData;
        if(achievementData == null|| achievementData.conditionData.Unlock())
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
    }
    public virtual void UpdatePanel()
    {
        if(achievementData ==null)
            return;
        titleText.text= achievementData.Title;
        descText.text= achievementData.Desc();
    }
    public abstract void OnClickedGetReward();
}