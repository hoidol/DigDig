using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class RewardPanel : MonoBehaviour
{    
    public Image thumImage;
    public TMP_Text rewardValueText;

    public void SetRewardData(RewardData rewardData, UserReward userReward)
    {
        thumImage.sprite= rewardData.GetThum();
        rewardValueText.text =rewardData.GetValueToString();
    }
}