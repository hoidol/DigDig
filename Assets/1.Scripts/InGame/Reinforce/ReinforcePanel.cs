using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReinforcePanel : MonoBehaviour
{
    public TMP_Text lvText;
    public TMP_Text priceText;
    public Image thumImage;
    SlimeData slimeData;

    public void SetSlimeData(SlimeData slimeData)
    {
        this.slimeData = slimeData;
        lvText.text = $"Lv.{GetReinforceLevel()}";
        priceText.text = GetPrice().ToString();
        thumImage.sprite = slimeData.thum;
    }

    int GetReinforceLevel()
    {
        return KingSlime.Instance.statMgr.GetReinforceLevel(slimeData.key);
    }

    // 레벨 0 → 30, 1 → 60, 2 → 90 ... (최대 레벨 없음)
    int GetPrice()
    {
        return GameSetting.REINFORCE_PRICE_PER_LEVEL * (GetReinforceLevel() + 1);
    }

    public void OnClickReinforce()
    {
        // LevelUpSlime은 필드에 그 종이 있을 때만 올라가므로, 없으면 코인도 쓰지 않음
        if (slimeData == null || !KingSlime.Instance.statMgr.HasSlimeOnField(slimeData.key))
            return;

        int price = GetPrice();
        if (KingSlime.Instance.coin < price)
        {
            ToastCanvas.Toast("Not enough coin");
            return;
        }

        KingSlime.Instance.AddCoin(-price);
        KingSlime.Instance.statMgr.LevelUpSlime(slimeData.key);
        SetSlimeData(slimeData);
    }
}
