using TMPro;
using UnityEngine;
using UnityEngine.UI;

// StatType 하나의 로비 업그레이드 (KSUpgradeContainer 하위에 미리 배치, upgradeType은 인스펙터에서 지정)
public class KSUpgradePanel : MonoBehaviour
{
    public StatType upgradeType;

    public Image statImage;
    public TMP_Text titleText;
    public TMP_Text lvText;
    public TMP_Text valueText; // 현재 업그레이드 값 → 다음 값
    public Image currencyImage;
    public TMP_Text priceText;
    public Button upgradeButton;

    public void UpdatePanel()
    {
        // 데이터(CSV 행)가 없는 StatType은 숨김
        KSUpgradeInfo info = KSUpgradeData.Instance.GetUpgradeInfo(upgradeType);
        gameObject.SetActive(info != null);
        if (info == null)
            return;

        int lv = UserDataManager.Instance.userKingSlimeManager.GetUpgrade(upgradeType).lv;
        bool isMaxLv = info.IsMaxLv(lv);

        if (statImage != null)
            statImage.sprite = StatData.GetThum(upgradeType);
        titleText.text = upgradeType.ToString();
        lvText.text = info.maxLv > 0 ? $"Lv.{lv}/{info.maxLv}" : $"Lv.{lv}";

        // 기본값 + 장비 + 업그레이드 합산 → 다음 Lv 합산
        KingSlimeData kingSlimeData = KingSlimeManager.Instance.GetKingSlimeData(UserDataManager.Instance.userData.CharacterName);
        float curValue = KingSlimeManager.Instance.GetTotalStatValue(kingSlimeData, upgradeType);
        float nextValue = curValue + info.GetValue(lv + 1) - info.GetValue(lv);
        valueText.text = isMaxLv
            ? StatData.GetValueToString(upgradeType, curValue)
            : $"{StatData.GetValueToString(upgradeType, curValue)} → {StatData.GetValueToString(upgradeType, nextValue)}";

        if (currencyImage != null)
            currencyImage.sprite = Resources.Load<Sprite>($"Icons/{info.currencyType}");

        int price = info.GetPrice(lv);
        priceText.text = isMaxLv ? "MAX" : price.ToString();
        upgradeButton.interactable = !isMaxLv && UserDataManager.Instance.userData.GetCurrency(info.currencyType) >= price;
    }

    public void OnClickedUpgradeButton()
    {
        if (!UserDataManager.Instance.userKingSlimeManager.TryUpgrade(upgradeType))
            return;

        // 재화가 줄어 다른 패널 버튼 상태도 바뀌고, 능력치 표시도 바뀌므로 캔버스 전체 갱신
        GetComponentInParent<KSManagementCanvas>().UpdateCanvas();
    }
}
