using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-22) 카드 줄(MinionOfferPanel) 왼쪽: 지금 킹 레벨 + 경험치 + 경험치 구매 버튼
// 버튼 = 주사위 코인 GameSetting.BUY_EXP_DICE_COIN_PRICE(2)개 → 경험치 BUY_EXP_AMOUNT(1) (KingSlime.TryBuyExp)
// 경험치는 구매로만 얻음(처치 경험치 없음). 경험치가 다 차면 바로 레벨업 창 (KingSlime.AddExp → LevelUpCanvas.OpenPending)
// 씬: MinionOfferPanel/KingExpBuyPanel (모양은 씬에서 수정). 없으면 MinionOfferSystem이 실행 중 기본 모양 생성(Build)
public class KingExpBuyPanel : MonoBehaviour
{
    public TMP_Text levelText;   // "Lv.N"
    // public TMP_Text expText;     // "exp/max"
    public Slider expSlider;     // (선택)
    public Button buyButton;
    public TMP_Text buyText;     // "EXP +1"
    public TMP_Text priceText;   // 주사위 코인 가격 (모자라면 빨간색)

    bool inited;

    void Start() => Init();

    // 이벤트 구독은 씬 Awake(GameEventBus.Clear)보다 늦게
    public void Init()
    {
        if (inited)
            return;
        inited = true;
        if (buyButton != null)
            buyButton.onClick.AddListener(OnClickedBuy);
        GameEventBus.Subscribe<ExpChangedEvent>(OnExpChanged);
        GameEventBus.Subscribe<DiceCoinEvent>(OnDiceCoin);
        Refresh();
    }

    void OnDestroy()
    {
        GameEventBus.Unsubscribe<ExpChangedEvent>(OnExpChanged);
        GameEventBus.Unsubscribe<DiceCoinEvent>(OnDiceCoin);
    }

    void OnEnable()
    {
        if (inited)
            Refresh();
    }

    void OnExpChanged(ExpChangedEvent e) => Refresh();
    void OnDiceCoin(DiceCoinEvent e) => Refresh();

    void OnClickedBuy()
    {
        KingSlime king = KingSlime.Instance;
        if (king == null)
            return;
        if (king.IsMaxLv)
        {
            ToastCanvas.Toast("최대 레벨입니다");
            return;
        }
        if (!king.TryBuyExp())
        {
            SoundManager.Play(SFXType.Fail_NotEnough);
            ToastCanvas.Toast("Not enough dice coin");
            return;
        }
        SoundManager.Play(SFXType.Click);
    }

    public void Refresh()
    {
        KingSlime king = KingSlime.Instance;
        if (king == null)
            return;
        if (levelText != null)
            levelText.text = $"Lv.{king.lv}"; // (2026-11-23) Lv.0 시작, 최대 Lv.9
        // if (expText != null)
        //     expText.text = $"{king.exp}/{king.MaxExp}";
        if (expSlider != null)
            expSlider.value = king.IsMaxLv ? 1f : king.MaxExp > 0 ? (float)king.exp / king.MaxExp : 0f;
        if (buyText != null)
            buyText.text = king.IsMaxLv ? "MAX" : $"EXP +{GameSetting.BUY_EXP_AMOUNT}";
        if (buyButton != null)
            buyButton.interactable = !king.IsMaxLv;
        bool affordable = king.diceCoin >= GameSetting.BUY_EXP_DICE_COIN_PRICE;
        if (priceText != null)
        {
            // 최대 레벨이면 가격 숨김 (가격 묶음(아이콘 + 숫자)이 따로 있으면 묶음째)
            Transform priceGroup = buyButton != null && priceText.transform.parent != buyButton.transform ? priceText.transform.parent : priceText.transform;
            priceGroup.gameObject.SetActive(!king.IsMaxLv);
            priceText.text = GameSetting.BUY_EXP_DICE_COIN_PRICE.ToString();
            priceText.color = affordable ? Color.white : Color.red;
        }
    }

    #region 기본 모양 (씬에 없을 때)
    // parent = MinionOfferPanel. 카드 줄 왼쪽 바깥에 붙임 (오른쪽 끝 = 카드 줄 왼쪽 끝 - gap)
    public static KingExpBuyPanel Build(RectTransform parent, TMP_FontAsset font, Sprite buttonSprite, Sprite diceCoinSprite)
    {
        const float WIDTH = 230f, GAP = 20f;
        var go = new GameObject("KingExpBuyPanel", typeof(RectTransform), typeof(KingExpBuyPanel));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(WIDTH, 0f);
        rt.anchoredPosition = new Vector2(-GAP, 0f);
        var panel = go.GetComponent<KingExpBuyPanel>();

        // 위: 레벨 + 경험치
        panel.levelText = NewText("LevelText", rt, font, 56, FontStyles.Bold, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -10), 64);
        // panel.expText = NewText("ExpText", rt, font, 34, FontStyles.Normal, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -76), 40);

        // 아래: 구매 버튼
        var btnGo = new GameObject("BuyExpButton", typeof(RectTransform), typeof(Image), typeof(Button));
        var btnRt = (RectTransform)btnGo.transform;
        btnRt.SetParent(rt, false);
        btnRt.anchorMin = new Vector2(0f, 0f);
        btnRt.anchorMax = new Vector2(1f, 0f);
        btnRt.pivot = new Vector2(0.5f, 0f);
        btnRt.sizeDelta = new Vector2(0f, 100f);
        btnRt.anchoredPosition = Vector2.zero;
        var bg = btnGo.GetComponent<Image>();
        bg.sprite = buttonSprite;
        bg.type = buttonSprite != null ? Image.Type.Sliced : Image.Type.Simple;
        bg.color = new Color(0.45f, 0.8f, 1f);
        panel.buyButton = btnGo.GetComponent<Button>();
        panel.buyText = NewText("BuyText", btnRt, font, 34, FontStyles.Bold, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -8), 42);

        // 가격 = 주사위 코인 아이콘 + 숫자
        var priceGo = new GameObject("PricePanel", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        var priceRt = (RectTransform)priceGo.transform;
        priceRt.SetParent(btnRt, false);
        priceRt.anchorMin = new Vector2(0f, 0f);
        priceRt.anchorMax = new Vector2(1f, 0f);
        priceRt.pivot = new Vector2(0.5f, 0f);
        priceRt.sizeDelta = new Vector2(0f, 44f);
        priceRt.anchoredPosition = new Vector2(0f, 6f);
        var layout = priceGo.GetComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 6;
        layout.childControlWidth = layout.childControlHeight = false;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        var iconGo = new GameObject("DiceCoinIcon", typeof(RectTransform), typeof(Image));
        var iconRt = (RectTransform)iconGo.transform;
        iconRt.SetParent(priceRt, false);
        iconRt.sizeDelta = new Vector2(40f, 40f);
        var icon = iconGo.GetComponent<Image>();
        icon.sprite = diceCoinSprite;
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        icon.enabled = diceCoinSprite != null;
        panel.priceText = NewText("PriceText", priceRt, font, 36, FontStyles.Bold, Vector2.zero, Vector2.zero, Vector2.zero, 44);
        ((RectTransform)panel.priceText.transform).sizeDelta = new Vector2(50f, 44f);
        panel.priceText.alignment = TextAlignmentOptions.MidlineLeft;
        return panel;
    }

    static TMP_Text NewText(string name, RectTransform parent, TMP_FontAsset font, float size, FontStyles style,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, float height)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(0f, height);
        rt.anchoredPosition = pos;
        var t = go.GetComponent<TextMeshProUGUI>();
        if (font != null)
            t.font = font;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = Color.white;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }
    #endregion
}
