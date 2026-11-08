using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrentKSAbilityInfoPanel : EquipmentAbilityInfoPanel
{
    public Image thumImage;
    float value = 0f;
    public void UpdatePanel(string characterName)
    {
        Init();
        titleText.text = StatData.GetTitle(statType);

        KingSlimeData playerData = KingSlimeManager.Instance.GetKingSlimeData(System.Enum.Parse<KingSlimeName>(characterName));
        thumImage.sprite = StatData.GetThum(statType);
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