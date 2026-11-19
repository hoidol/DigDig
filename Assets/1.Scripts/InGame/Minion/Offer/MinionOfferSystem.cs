using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-19) wave 카드 소환: 골드 소환 대신 wave가 시작될 때마다 장착 5종 중 랜덤 OFFER_COUNT장(중복 가능)을 하단에 지급.
// 카드를 원하는 타일로 드래그 → 빈 칸이면 0레벨 소환, 같은 종·0레벨 미니언 위면 바로 합성(같은 종 +1).
// wave 중에는 새 카드를 받을 수 없음(다음 wave 시작 때 지급). 쓰지 않은 카드는 새 카드를 줄 때 지움(KEEP_UNUSED_OFFERS).
// 카드를 놓으려면 주사위 코인 OFFER_MINION_DICE_COIN_PRICE(2)개 필요. 브레이크타임마다 WAVE_DICE_COIN(5)개 지급, 남은 코인은 이월.
// UI = 씬 InGame의 BottomGameCanvas/Panel/Buttons_Hor/MinionOfferPanel (카드 템플릿·Next 버튼을 인스펙터에서 연결).
// 씬에 없거나 연결이 비면 실행 중에 기본 모양을 만든다(BuildUI)
public class MinionOfferSystem : MonoBehaviour
{
    public const int OFFER_COUNT = 3;
    public const int MAX_SAME_OFFER = 2; // 한 번에 나오는 카드 중 같은 종 최대 장수 (2026-11-19)
    public const bool KEEP_UNUSED_OFFERS = false; // (2026-11-19) 새 카드를 줄 때 남은 카드는 지움. true면 남은 카드에 3장 추가

    public static MinionOfferSystem Instance { get; private set; }

    public readonly List<string> hand = new List<string>(); // 들고 있는 카드(미니언 key). 사용한 칸은 null (카드 자리가 밀리지 않게)

    [Header("씬 연결 (비우면 실행 중 생성)")]
    public Canvas canvas;                 // 드래그 그림을 그릴 캔버스 (비우면 상위 캔버스)
    public RectTransform cardContainer;   // 카드가 놓일 곳 (HorizontalLayoutGroup 권장)
    public MinionOfferCard cardTemplate;  // 카드 하나 (꺼 둔 채로 두면 복사해서 씀)
    public Button nextButton;             // 브레이크타임 Next 버튼
    readonly List<MinionOfferCard> cards = new List<MinionOfferCard>();

    // GameManager.Start에서 호출: 씬에 있으면 그것, 없으면 새로 만들어 준비
    public static MinionOfferSystem Create()
    {
        MinionOfferSystem system = Instance != null ? Instance : FindFirstObjectByType<MinionOfferSystem>(FindObjectsInactive.Include);
        if (system == null)
            system = new GameObject("MinionOfferSystem").AddComponent<MinionOfferSystem>();
        system.gameObject.SetActive(true);
        system.Init();
        return system;
    }

    bool inited;

    // 이벤트 구독은 Awake가 아니라 여기서 (씬 Awake의 GameEventBus.Clear보다 늦게)
    void Init()
    {
        if (inited)
            return;
        inited = true;
        Instance = this;
        if (cardContainer == null || cardTemplate == null || nextButton == null)
            BuildUI();
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>()?.rootCanvas;
        cardTemplate.gameObject.SetActive(false);
        nextButton.onClick.AddListener(OnClickedNext);
        nextButton.gameObject.SetActive(false);
        HideGoldUI();
        SetPanelActive(GameManager.Instance != null && GameManager.Instance.isBreakTime);
        GameEventBus.Subscribe<BreakTimeStartEvent>(OnBreakTimeStart);
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStart);
        GameEventBus.Subscribe<DiceCoinEvent>(OnDiceCoin);
    }

    void OnDestroy()
    {
        GameEventBus.Unsubscribe<BreakTimeStartEvent>(OnBreakTimeStart);
        GameEventBus.Unsubscribe<WaveStartEvent>(OnWaveStart);
        GameEventBus.Unsubscribe<DiceCoinEvent>(OnDiceCoin);
        if (Instance == this)
            Instance = null;
    }

    int lastDealtWaveIdx = -1; // 부활로 같은 wave 브레이크타임가 다시 와도 카드를 또 주지 않음

    // wave 클리어(또는 게임 시작) → 다음 wave 브레이크타임: 카드 3장 + 주사위 코인 WAVE_DICE_COIN개
    void OnBreakTimeStart(BreakTimeStartEvent e)
    {
        SetPanelActive(true); // 부활로 같은 wave 브레이크타임가 다시 와도 패널은 켬
        if (e.nextWaveIdx <= lastDealtWaveIdx)
            return;
        lastDealtWaveIdx = e.nextWaveIdx;
        // (2026-11-20) 브레이크타임마다 주사위 코인을 WAVE_DICE_COIN개로 초기화 (남은 코인은 이월하지 않음)
        // (2026-11-20) + 보너스 엘리트 처치로 모아 둔 코인 (받으면 0으로)
        KingSlime king = KingSlime.Instance;
        int bonus = king.bonusDiceCoinNextBreak;
        king.bonusDiceCoinNextBreak = 0;
        king.AddDiceCoin(GameSetting.WAVE_DICE_COIN + bonus - king.diceCoin);
        DealOffers();
    }

    // (2026-11-19) wave 중엔 카드 패널을 끄고(카드 사용 불가), wave가 끝나 브레이크타임가 되면 다시 켬
    void OnWaveStart(WaveStartEvent e)
    {
        SetPanelActive(false);
        nextButton.gameObject.SetActive(false); // Next 표시는 이 오브젝트의 Update가 하므로 꺼지기 전에 직접 숨김
    }

    // 패널 = 이 오브젝트(MinionOfferPanel). 실행 중 기본 모양(BuildUI)이면 Next 버튼이 하위에 있으므로 카드 줄만
    void SetPanelActive(bool active)
    {
        GameObject panel = nextButton.transform.IsChildOf(transform) ? cardContainer.gameObject : gameObject;
        if (panel.activeSelf != active)
            panel.SetActive(active);
    }

    // 카드 가격 = 미니언마다 MinionData.offerDiceCoinPrice (주사위 코인)
    public static int GetCardPrice(string key)
    {
        MinionData data = MinionManager.Instance.GetMinionData(key);
        return data != null ? data.offerDiceCoinPrice : GameSetting.OFFER_MINION_DICE_COIN_PRICE;
    }
    public static bool CanAffordCard(string key) => KingSlime.Instance.diceCoin >= GetCardPrice(key);

    // 코인이 바뀌면 카드 표시(가격·흐림) 갱신
    void OnDiceCoin(DiceCoinEvent e) => RefreshCards();

    public bool IsCardDragging { get; set; } // MinionOfferCard가 드래그 중

    // Next 버튼: 브레이크타임 + 카드·유닛 드래그 중이 아닐 때만
    void Update()
    {
        if (!inited || nextButton == null || GameManager.Instance == null)
            return;
        bool show = GameManager.Instance.isPlaying && GameManager.Instance.isBreakTime
            && !IsCardDragging && !MinionFieldController.Instance.IsDragging;
        if (nextButton.gameObject.activeSelf != show)
            nextButton.gameObject.SetActive(show);
    }

    // Next: 아직 살 수 있는 카드가 남았으면(주사위 코인으로 살 수 있는 카드) 확인 팝업 → Yes면 wave 시작
    void OnClickedNext()
    {
        SoundManager.Play(SFXType.NextWaveButton);
        if (HasAffordableCard() && YesOrNoCanvas.Instance != null)
        {
            YesOrNoCanvas.Instance.OpenCanvas(TranslateManager.GetText("NextWave_DiceCoinLeft_Title"), TranslateManager.GetText("NextWave_DiceCoinLeft_Body"), yes =>
            {
                if (yes)
                    GameManager.Instance.EndBreakTime();
            });
            return;
        }
        GameManager.Instance.EndBreakTime();
    }

    // 손에 남은 카드 중 지금 주사위 코인으로 살 수 있는 카드가 있음 (= 코인을 다 쓰지 않음)
    public bool HasAffordableCard()
    {
        foreach (string key in hand)
            if (key != null && CanAffordCard(key))
                return true;
        return false;
    }

    // 장착 5종 중 랜덤 OFFER_COUNT장 (같은 종이 겹칠 수 있음)
    public void DealOffers()
    {
        if (!KEEP_UNUSED_OFFERS)
            hand.Clear();
        else
            hand.RemoveAll(k => k == null);
        UserMinion[] equipped = UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions;
        // 같은 종은 한 번에 MAX_SAME_OFFER장까지 (라이트닝 3장 같은 건 안 나옴)
        Dictionary<string, int> dealt = new Dictionary<string, int>();
        List<string> candidates = new List<string>();
        for (int i = 0; i < OFFER_COUNT; i++)
        {
            candidates.Clear();
            foreach (UserMinion m in equipped)
                if (m != null && (!dealt.TryGetValue(m.key, out int c) || c < MAX_SAME_OFFER))
                    candidates.Add(m.key);
            if (candidates.Count == 0)
                break;
            string key = candidates[Random.Range(0, candidates.Count)];
            dealt[key] = dealt.TryGetValue(key, out int n) ? n + 1 : 1;
            hand.Add(key);
        }
        RefreshCards();
        ArrangeCards();
    }

    // 카드 배치: 지급할 때만 HorizontalLayoutGroup으로 자리를 잡고 바로 끔 → 카드를 쓰거나 드래그해도 나머지 카드는 제자리
    bool needArrange; // 카드 줄이 꺼져 있을 때 지급되면 켜질 때 배치 (꺼진 자식은 레이아웃이 무시함)

    void OnEnable()
    {
        if (needArrange)
            ArrangeCards();
    }

    void ArrangeCards()
    {
        HorizontalLayoutGroup layout = cardContainer.GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
            return;
        needArrange = !cardContainer.gameObject.activeInHierarchy;
        if (needArrange)
            return;
        layout.enabled = true;
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardContainer);
        layout.enabled = false;
    }

    // 카드를 worldPos에 놓기. 성공하면 카드 사용
    public bool TryPlace(int handIdx, Vector2 worldPos)
    {
        if (handIdx < 0 || handIdx >= hand.Count || hand[handIdx] == null)
            return false;
        string key = hand[handIdx];
        Tile tile = TileManager.Instance.GetNearestTile(worldPos);
        if (tile == null)
            return false;
        int price = GetCardPrice(key);
        if (KingSlime.Instance.diceCoin < price)
        {
            ToastCanvas.Toast("Not enough dice coin");
            return false;
        }

        if (tile.IsEmpty)
        {
            MinionFieldController field = MinionFieldController.Instance;
            if (field.minionMaxCount <= field.CountedMinionCount)
            {
                ToastCanvas.Toast("Max Minion");
                return false;
            }
            Minion minion = KingSlime.Instance.AddMinion(key, 0);
            tile.SetTileObject(minion);
            SoundManager.Play(SFXType.MinionAppear);
            AchievementManager.Report(AchievementType.SpawnMinion);
        }
        else if (tile.tileObject is Minion target && CanMergeCard(key, target))
        {
            Minion merged = MinionFieldController.Instance.ReplaceMinion(target, key, target.mergeLevel + 1);
            merged.PlayMergeHop();
            if (EffectManager.Instance != null)
                EffectManager.Instance.Play(EffectType.Merge, merged.transform.position);
            SoundManager.Play(SFXType.MinionMerge);
            MinionFieldController.Instance.mergeCount++;
            AchievementManager.Report(AchievementType.MergeMinion);
            GameEventBus.Publish(new MergedMinionEvent());
        }
        else
        {
            return false;
        }

        hand[handIdx] = null; // 자리 유지 (RemoveAt하면 뒤 카드가 앞으로 당겨짐)
        KingSlime.Instance.AddDiceCoin(-price); // DiceCoinEvent → RefreshCards
        KingSlimeManageCanvas.Instance.UpdateCanvas();
        return true;
    }

    // 카드(0레벨)는 같은 종·0레벨 미니언과 합성
    static bool CanMergeCard(string key, Minion target)
    {
        MinionData data = MinionManager.Instance.GetMinionData(key);
        return Minion.CanMerge(key, 0, data.growth, target.key, target.mergeLevel, target.MinionData.growth);
    }

    #region UI 기본 모양 (씬에 연결이 없을 때만 실행 중 생성)
    const float CARD_SIZE = 200f;
    const float PANEL_HEIGHT = 240f;
    const float PANEL_BOTTOM = 40f;

    void BuildUI()
    {
        GameObject canvasGo = new GameObject("MinionOfferCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = new GameObject("CardContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        cardContainer = panel.GetComponent<RectTransform>();
        cardContainer.SetParent(canvasGo.transform, false);
        cardContainer.anchorMin = new Vector2(0, 0);
        cardContainer.anchorMax = new Vector2(1, 0);
        cardContainer.pivot = new Vector2(0.5f, 0);
        cardContainer.sizeDelta = new Vector2(0, PANEL_HEIGHT);
        cardContainer.anchoredPosition = new Vector2(0, PANEL_BOTTOM);
        HorizontalLayoutGroup layout = panel.GetComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 24;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        cardTemplate = BuildDefaultCard(cardContainer, canvas);
        nextButton = BuildDefaultNextButton(canvasGo.transform, new Vector2(0, PANEL_BOTTOM + PANEL_HEIGHT + 20));
    }

    // 기본 카드 (흰 판 + 미니언 그림). 에디터에서 씬 UI를 만들 때도 사용
    public static MinionOfferCard BuildDefaultCard(Transform parent, Canvas rootCanvas, Sprite bgSprite = null)
    {
        GameObject go = new GameObject("MinionOfferCard", typeof(RectTransform), typeof(Image), typeof(MinionOfferCard));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.sizeDelta = new Vector2(CARD_SIZE, CARD_SIZE);
        Image bg = go.GetComponent<Image>();
        bg.sprite = bgSprite;
        bg.type = bgSprite != null ? Image.Type.Sliced : Image.Type.Simple;
        bg.color = new Color(1f, 1f, 1f, 0.9f);

        GameObject thumGo = new GameObject("Thum", typeof(RectTransform), typeof(Image));
        RectTransform thumRt = thumGo.GetComponent<RectTransform>();
        thumRt.SetParent(rt, false);
        thumRt.anchorMin = Vector2.zero;
        thumRt.anchorMax = Vector2.one;
        thumRt.offsetMin = new Vector2(16, 16);
        thumRt.offsetMax = new Vector2(-16, -16);
        Image thum = thumGo.GetComponent<Image>();
        thum.preserveAspect = true;
        thum.raycastTarget = false;

        MinionOfferCard card = go.GetComponent<MinionOfferCard>();
        card.thumImage = thum;
        card.rootCanvas = rootCanvas;
        return card;
    }

    // 기본 Next 버튼 (주황 판 + "Next")
    public static Button BuildDefaultNextButton(Transform parent, Vector2 pos, Sprite bgSprite = null, TMPro.TMP_FontAsset font = null)
    {
        GameObject go = new GameObject("NextWaveButton", typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.sizeDelta = new Vector2(300, 150);
        rt.anchoredPosition = pos;
        Image bg = go.GetComponent<Image>();
        bg.sprite = bgSprite;
        bg.type = bgSprite != null ? Image.Type.Sliced : Image.Type.Simple;
        bg.color = new Color(1f, 0.75f, 0.2f);

        GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.SetParent(rt, false);
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = textRt.offsetMax = Vector2.zero;
        TMPro.TextMeshProUGUI text = textGo.GetComponent<TMPro.TextMeshProUGUI>();
        if (font != null)
            text.font = font;
        text.text = "Next";
        text.fontSize = 64;
        text.color = Color.white;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return go.GetComponent<Button>();
    }
    #endregion

    // 카드는 템플릿을 복사해서 씀 (모자라면 추가)
    void RefreshCards()
    {
        while (cards.Count < hand.Count)
        {
            MinionOfferCard card = Instantiate(cardTemplate, cardContainer);
            card.name = $"MinionOfferCard_{cards.Count}";
            if (card.rootCanvas == null)
                card.rootCanvas = canvas;
            cards.Add(card);
        }
        for (int i = 0; i < cards.Count; i++)
        {
            bool active = i < hand.Count && hand[i] != null;
            cards[i].gameObject.SetActive(active);
            if (active)
                cards[i].SetData(this, i, MinionManager.Instance.GetMinionData(hand[i]), GetCardPrice(hand[i]), CanAffordCard(hand[i]));
        }
    }

    // 골드 개념 삭제: 골드로 하는 소환·강화와 코인 표시를 숨김 (씬은 그대로, 실행 중에만)
    static void HideGoldUI()
    {
        foreach (MinionSpawnButton b in FindObjectsByType<MinionSpawnButton>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            b.gameObject.SetActive(false);
        foreach (ReinforceContainer c in FindObjectsByType<ReinforceContainer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            c.gameObject.SetActive(false);
        foreach (CoinPanel p in FindObjectsByType<CoinPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            p.gameObject.SetActive(false);
    }
}
