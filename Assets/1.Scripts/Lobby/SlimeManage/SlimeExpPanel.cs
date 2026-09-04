using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlimeExpPanel : MonoBehaviour
{
    public Image expBar;
    public TMP_Text expText;
    public void SetSlime(SlimeData slimeData)
    {
        UserSlime userSlime = UserManager.Instance.userSlimeManager.GetUserSlime(slimeData.key);
        
        EnhanceExpInfo enhanceExpInfo = SlimeManager.Instance.GetEnhanceExpInfo(slimeData.grade);
        int totalPreLvSumExp = enhanceExpInfo.TotalExp(userSlime.enhanceLevel-1);
        int exp = userSlime.exp - totalPreLvSumExp;
        expText.text = $"{exp}/{enhanceExpInfo.exps[userSlime.enhanceLevel]}";
        expBar.fillAmount = exp/enhanceExpInfo.exps[userSlime.enhanceLevel];
        
    }
}