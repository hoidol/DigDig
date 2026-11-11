using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 장착 슬라임 1종의 인게임 강화 칸 (ReinforceCanvas.reinforcePanels). 가격 = 30 × (현재 레벨 + 1)
// 강화는 종(key) 단위 → 필드에 그 종이 없어도 강화 가능, 이후 소환되는 슬라임에도 적용
public class ReinforcePanel : MonoBehaviour
{
    public TMP_Text lvText;
    public TMP_Text priceText;
    public Image thumImage;
    public Button reinforceButton; // 연결하면 클릭 시 강화 (인스펙터 OnClick에 OnClickReinforce를 따로 걸지 말 것)
    SlimeData slimeData;

    public SlimeData SlimeData => slimeData;

    void Awake()
    {
        if (reinforceButton != null)
            reinforceButton.onClick.AddListener(OnClickReinforce);
    }

    public void SetSlimeData(SlimeData slimeData)
    {
        this.slimeData = slimeData;
        if (lvText != null)
            lvText.text = $"Lv.{GetReinforceLevel()}";
        if (priceText != null)
            priceText.text = GetPrice().ToString();
        if (thumImage != null)
            thumImage.sprite = slimeData.thum;
    }

    int GetReinforceLevel()
    {
        return KingSlime.Instance.statMgr.GetReinforceLevel(slimeData.key);
    }

    // 레벨 0 → 30, 1 → 60, 2 → 90 ... (최대 레벨 없음)
    public static int GetPrice(int reinforceLevel) => GameSetting.REINFORCE_PRICE_PER_LEVEL * (reinforceLevel + 1);
    public int GetPrice() => GetPrice(GetReinforceLevel());

    public void OnClickReinforce()
    {
        if (slimeData == null)
            return;

        int price = GetPrice();
        if (KingSlime.Instance.coin < price)
        {
            ToastCanvas.Toast(TranslateManager.GetText("Not enough coin"));
            return;
        }

        KingSlime.Instance.AddCoin(-price);
        KingSlime.Instance.statMgr.LevelUpSlime(slimeData.key);
        SetSlimeData(slimeData);
    }
}
