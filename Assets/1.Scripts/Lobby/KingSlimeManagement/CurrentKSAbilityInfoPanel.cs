using DG.Tweening;
using TMPro;
using UnityEngine;

public class CurrentKSAbilityInfoPanel : EquipmentAbilityInfoPanel
{
    float value = 0f;
    public void UpdatePanel(string characterName)
    {
        Init();
        titleText.text = statType.ToString();

        KingSlimeData playerData = KingSlimeManager.Instance.GetKingSlimeData(System.Enum.Parse<KingSlimeName>(characterName));

        float preValue = value;
        value = KingSlimeManager.Instance.GetTotalStatValue(playerData, statType);

        if (preValue != value)
        {
            valueText.transform.DOKill();
            valueText.transform.DOScale(Vector3.one * 1.2f, 0.1f).OnComplete(() =>
            {
                valueText.transform.DOScale(Vector3.one, 0.1f);
            });
        }

        valueText.text = StatData.GetValueToString(statType, value);
    }
}