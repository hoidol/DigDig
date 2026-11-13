using UnityEngine.UI;
using TMPro;
using UnityEngine;

// 업적 한 칸: 이름, 진행도(슬라이더 + 글씨), 보상, 상태 오브젝트(claimObj 등), 받을 수 있으면 redDot
public abstract class AchievementPanel : MonoBehaviour
{
    public TMP_Text titleText; //스톤 부수기
    // public TMP_Text descText; //부순 횟수 {0}/{goal}
    public Slider slider; // 진행도 바 (0~1)
    public TMP_Text progressText;
    public TMP_Text rewardValueText;
    public Image rewardImage;     // 보상 아이콘 (없으면 생략)
    public GameObject claimObj;   // 보상을 받을 수 있을 때
    public GameObject redDot;     // 보상을 받을 수 있을 때

    public AchievementData achievementData { get; private set; }
    public virtual void SetAchievementData(AchievementData aData)
    {
        achievementData = aData;
        if (achievementData == null || !achievementData.conditionData.Unlock())
        {
            achievementData = null;
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
    }
    public virtual void UpdatePanel()
    {
        if (achievementData == null)
            return;

        titleText.text = achievementData.Title;
        progressText.text = achievementData.Desc();
        slider.value = achievementData.fillAmount;

        rewardValueText.text = achievementData.rewardData.GetValueToString();
        if (rewardImage != null)
        {
            Sprite thum = achievementData.rewardData.GetThum();
            if (thum != null)
                rewardImage.sprite = thum;
        }
    }

    // 상태 표시 오브젝트 켜기/끄기 (연결 안 된 것은 건너뜀)
    protected static void SetStateObj(GameObject obj, bool active)
    {
        if (obj != null)
            obj.SetActive(active);
    }

    public abstract void OnClickedGetReward();
}
