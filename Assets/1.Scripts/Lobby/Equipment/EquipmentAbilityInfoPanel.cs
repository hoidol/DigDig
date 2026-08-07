using TMPro;
using UnityEngine;

//장비의 개별 능력치 보여주는 창
public class EquipmentAbilityInfoPanel : MonoBehaviour
{
    [Header("보여줄 스텟 미리 설정하기")]
    public StatType statType;
    public TMP_Text titleText;
    public TMP_Text valueText;
    public virtual void UpdatePanel(EquipmentData equipmentData)
    {
        EquipmentAbility equipmentAbility = equipmentData.GetEquipmentAbility(statType);
        if (equipmentAbility != null)
        {
            gameObject.SetActive(true);
            titleText.text = equipmentAbility.Title;
            valueText.text = equipmentAbility.GetValueToString();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}