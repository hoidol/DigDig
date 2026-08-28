using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MineMeExpPanel : MonoBehaviour
{
    public Image expBar;
    public TMP_Text expText;
    public void SetMiniMe(MiniMeData miniMeData)
    {
        UserMiniMe userMiniMe = UserManager.Instance.userMiniMeManager.GetUserMiniMe(miniMeData.key);
        
        EnhanceExpInfo enhanceExpInfo = MiniMeManager.Instance.GetEnhanceExpInfo(miniMeData.grade);
        int totalPreLvSumExp = enhanceExpInfo.TotalExp(userMiniMe.enhanceLevel-1);
        int exp = userMiniMe.exp - totalPreLvSumExp;
        expText.text = $"{exp}/{enhanceExpInfo.exps[userMiniMe.enhanceLevel]}";
        expBar.fillAmount = exp/enhanceExpInfo.exps[userMiniMe.enhanceLevel];
        
    }
}