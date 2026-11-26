using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 킹 능력치 한 줄 (기본 + 레벨 스탯 포인트)
public class CurrentKSAbilityInfoPanel : MonoBehaviour
{
    [Header("보여줄 스탯 미리 설정하기")]
    public StatType statType;
    public TMP_Text titleText;
    public TMP_Text valueText;
    public Image thumImage;
    float value = 0f;

    void Init()
    {
        if (titleText == null)
            titleText = transform.Find("TitleText").GetComponent<TMP_Text>();
        if (valueText == null)
            valueText = transform.Find("ValueText").GetComponent<TMP_Text>();
    }

    // hero가 있으면 그 용사 Lv.1 스탯(HeroData.csv, 없는 스탯은 킹 기본) + 수련 포인트 (2026-11-26 용사 탭)
    public void UpdatePanel(string characterName, HeroDataTable.HeroData hero = null)
    {
        Init();
        titleText.text = StatData.GetTitle(statType);

        KingSlimeData playerData = KingSlimeManager.Instance.GetKingSlimeData(System.Enum.Parse<KingSlimeName>(characterName));
        thumImage.sprite = StatData.GetThum(statType);
        float preValue = value;
        value = hero != null && hero.TryGetStat(statType, 1, out float heroBase)
            ? KingSlimeManager.Instance.GetTotalStatValue(heroBase, statType)
            : KingSlimeManager.Instance.GetTotalStatValue(playerData, statType);

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
