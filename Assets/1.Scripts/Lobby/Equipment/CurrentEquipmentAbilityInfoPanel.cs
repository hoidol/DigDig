using DG.Tweening;
using TMPro;
using UnityEngine;

public class CurrentEquipmentAbilityInfoPanel : MonoBehaviour
{
    public StatType statType;

    public TMP_Text titleText;
    public TMP_Text valueText;
    float value = 0f;
    public void UpdatePanel(string characterName)
    {
        titleText.text = statType.ToString();

        CharacterData playerData = Resources.Load<CharacterData>($"PlayerData/{characterName}");
        
        float preValue= value;
        value = EquipmentManager.Instance.GetTotalStatValue(playerData, statType);

        if(preValue != value)
        {
            valueText.transform.DOKill();
            valueText.transform.DOScale(Vector3.one * 1.2f, 0.1f).OnComplete(() =>
            {
                valueText.transform.DOScale(Vector3.one, 0.1f);
            });
        }

        valueText.text = StatData.GetValueToString(statType,value);
    }
}