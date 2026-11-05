using TMPro;
using UnityEngine;

//장비의 개별 능력치 보여주는 창
public class EquipmentAbilityInfoPanel : MonoBehaviour
{
    [Header("보여줄 스텟 미리 설정하기")]
    public StatType statType;
    public TMP_Text titleText;
    public TMP_Text valueText;
    public void Init()
    {
        if (titleText == null)
            titleText = transform.Find("TitleText").GetComponent<TMP_Text>();
        if (valueText == null)
            valueText = transform.Find("ValueText").GetComponent<TMP_Text>();
    }
    public virtual void UpdatePanel(EquipmentData equipmentData)
    {
        Init();
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