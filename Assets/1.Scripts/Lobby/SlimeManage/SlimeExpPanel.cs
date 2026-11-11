using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlimeExpPanel : MonoBehaviour
{
    // public Image expBar;
    public Slider slider;
    public TMP_Text expText;
    public void SetSlime(SlimeData slimeData)
    {
        UserSlime userSlime = UserDataManager.Instance.userSlimeManager.GetUserSlime(slimeData.key);
        
        EnhanceExpInfo enhanceExpInfo = SlimeManager.Instance.GetEnhanceExpInfo(slimeData.grade);
        int totalPreLvSumExp = enhanceExpInfo.TotalExp(userSlime.enhanceLevel);
        int exp = userSlime.exp - totalPreLvSumExp;
        expText.text = $"{exp}/{enhanceExpInfo.exps[userSlime.enhanceLevel]}";
        int needExp = enhanceExpInfo.exps[userSlime.enhanceLevel];
        // expBar.fillAmount = needExp > 0 ? (float)exp / needExp : 1f; // 필요 경험치 0(데이터 없음·최대)이면 가득
        slider.value = needExp > 0 ? (float)exp / needExp : 1f;
        
    }
}