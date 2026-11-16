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
        
        // 모인 경험치 / 이번 레벨 필요량 (레벨업하면 필요량만큼 소모). 레벨은 등급별 시작 레벨 보정(EnhanceLevel)
        EnhanceExpInfo enhanceExpInfo = MinionManager.Instance.GetEnhanceExpInfo(minionData.grade);
        int lv = userMinion.EnhanceLevel;
        if (enhanceExpInfo == null || enhanceExpInfo.IsMaxLevel(lv))
        {
            if (expText != null)
                expText.text = "MAX";
            if (slider != null)
                slider.value = 1f;
            return;
        }
        int needExp = enhanceExpInfo.exps[lv];
        if (expText != null)
            expText.text = $"{userMinion.exp}/{needExp}";
        if (slider != null)
            slider.value = needExp > 0 ? Mathf.Clamp01((float)userMinion.exp / needExp) : 1f;
        
    }
}