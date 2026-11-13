using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinionExpPanel : MonoBehaviour
{
    // public Image expBar;
    public Slider slider;
    public TMP_Text expText;
    public void SetMinion(MinionData minionData)
    {
        UserMinion userMinion = UserDataManager.Instance.userMinionManager.GetUserMinion(minionData.key);
        
        EnhanceExpInfo enhanceExpInfo = MinionManager.Instance.GetEnhanceExpInfo(minionData.grade);
        int totalPreLvSumExp = enhanceExpInfo.TotalExp(userMinion.enhanceLevel);
        int exp = userMinion.exp - totalPreLvSumExp;
        expText.text = $"{exp}/{enhanceExpInfo.exps[userMinion.enhanceLevel]}";
        int needExp = enhanceExpInfo.exps[userMinion.enhanceLevel];
        // expBar.fillAmount = needExp > 0 ? (float)exp / needExp : 1f; // 필요 경험치 0(데이터 없음·최대)이면 가득
        slider.value = needExp > 0 ? (float)exp / needExp : 1f;
        
    }
}