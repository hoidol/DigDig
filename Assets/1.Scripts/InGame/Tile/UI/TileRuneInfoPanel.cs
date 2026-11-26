using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 선택된 타일의 아이템 이름/설명. 아이템 없는 타일은 표시 안 함
// (2026-11-22) 룬 강화(UpgradeTileItemButton)·제거(SellTileItemButton) 버튼 삭제
// (2026-11-26) 룬 강화 다시: 브레이크타임에 룬 칸을 누르면 강화 버튼 → 주사위 코인 RUNE_UPGRADE_DICE_COIN(3)개로 레벨 +1 (확정, 최대 레벨까지)
//   wave 중·최대 레벨·강화 불가 룬이면 버튼 숨김. 버튼 = upgradeButton (씬에 있음: 메뉴 Tools/InGame/Add Rune Upgrade Button To Info Panel. 실행 중 생성 없음)
// (2026-11-21) 킹 버프 칸(빈 칸·미니언이 있는 칸)이면 같은 자리에 버프 칸 정보: 종족·직업·패시브가 주는 버프 (EliteArea.GetEliteDescription)
//   룬이 있는 버프 칸은 룬 설명 아래에 버프 칸 정보를 덧붙임
public class TileRuneInfoPanel : MonoBehaviour
{
    public GameObject obj;
    public TMP_Text lvText;
    public TMP_Text descText;
    public Image thumImage;

    [Header("룬 강화 (2026-11-26, 씬에 만들어 연결: Tools/InGame/Add Rune Upgrade Button To Info Panel)")]
    public Button upgradeButton;
    public TMP_Text upgradeText;

    Tile currentTile;

    void Start()
    {
        obj.SetActive(false);
    }

    void OnEnable()
    {
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStart);
        GameEventBus.Subscribe<BreakTimeStartEvent>(OnBreakTimeStart);
        GameEventBus.Subscribe<DiceCoinEvent>(OnDiceCoin);
    }

    void OnDisable()
    {
        GameEventBus.Unsubscribe<WaveStartEvent>(OnWaveStart);
        GameEventBus.Unsubscribe<BreakTimeStartEvent>(OnBreakTimeStart);
        GameEventBus.Unsubscribe<DiceCoinEvent>(OnDiceCoin);
    }

    void OnWaveStart(WaveStartEvent e) => RefreshUpgradeButton();
    void OnBreakTimeStart(BreakTimeStartEvent e) => RefreshUpgradeButton();
    void OnDiceCoin(DiceCoinEvent e) => RefreshUpgradeButton();

    public void SetTile(Tile tile)
    {
        currentTile = tile;
        bool isBuffTile = EliteArea.IsEliteTile(tile);
        if (tile != null && tile.tileRune is not TileRune && isBuffTile)
        {
            SetBuffTile();
            RefreshUpgradeButton();
            return;
        }
        // 아이템이 설치된 타일만 표시 (위에 유닛이 있어도 아이템 없으면 숨김)
        if (tile == null || tile.tileRune is not TileRune rune)
        {
            obj.SetActive(false);
            return;
        }
        RuneData data = rune.runeData;
        // 강화 가능한 룬은 이름 옆에 레벨
        lvText.text = rune.CanUpgrade ? $"{data.Title} Lv.{rune.upgradeLevel}" : data.Title;
        descText.text = rune.GetDescription();
        if (isBuffTile)
            descText.text += $"\n<size=85%>{EliteArea.Current.GetEliteDescription()}</size>";
        thumImage.sprite = data.thum;
        thumImage.gameObject.SetActive(data.thum != null);
        obj.SetActive(true);
        RefreshUpgradeButton();
    }

    // 룬 없는 버프 칸: 제목 "정예병 칸", 그림 = 용사 아이콘
    void SetBuffTile()
    {
        lvText.text = "정예병 칸"; // (2026-11-24) 버프 칸 → 정예병 칸
        descText.text = EliteArea.Current.GetEliteDescription();
        HeroKingAbility hero = KingSlime.Instance.abilityInventory.Get<HeroKingAbility>();
        Sprite icon = hero != null && hero.data != null ? hero.data.thum : null;
        thumImage.sprite = icon;
        thumImage.gameObject.SetActive(icon != null);
        obj.SetActive(true);
    }

    public void UpdatePanel()
    {

    }

    #region 룬 강화
    TileRune CurrentRune => currentTile != null ? currentTile.tileRune as TileRune : null;

    // 강화 가능 = 브레이크타임 + 강화 가능한 룬 + 최대 레벨 아님
    public static bool CanUpgradeNow(TileRune rune) => rune != null && rune.CanUpgrade && !rune.IsMaxLevel
        && GameManager.Instance != null && GameManager.Instance.isBreakTime;

    void RefreshUpgradeButton()
    {
        TileRune rune = CurrentRune;
        bool show = obj != null && obj.activeSelf && CanUpgradeNow(rune);
        if (upgradeButton == null)
            return; // 씬에 연결 (Tools/InGame/Add Rune Upgrade Button To Info Panel)
        if (!upgradeHooked)
        {
            upgradeHooked = true;
            upgradeButton.onClick.AddListener(OnClickedUpgrade);
        }
        upgradeButton.gameObject.SetActive(show);
        if (!show)
            return;
        int price = GameSetting.RUNE_UPGRADE_DICE_COIN;
        bool enough = KingSlime.Instance != null && KingSlime.Instance.diceCoin >= price;
        if (upgradeText != null)
            upgradeText.text = $"강화 Lv.{rune.upgradeLevel + 1}";
    }

    public void OnClickedUpgrade()
    {
        TileRune rune = CurrentRune;
        KingSlime king = KingSlime.Instance;
        if (!CanUpgradeNow(rune) || king == null)
            return;
        int price = GameSetting.RUNE_UPGRADE_DICE_COIN;
        if (king.diceCoin < price)
        {
            ToastCanvas.Toast("Not enough dice coin");
            return;
        }
        king.AddDiceCoin(-price);
        if (rune.ForceUpgrade())
        {
            SoundManager.Play(SFXType.TileItemUpgradeSuccess);
            EffectManager.PlayPopText(rune.tile.transform.position + Vector3.up * 0.4f, PopTextType.Success);
        }
        SetTile(currentTile); // 레벨·설명·버튼 갱신
    }

    // 기본 모양: 패널(obj) 아래 가운데 버튼
    bool upgradeHooked;

    // 에디터 빌더가 씬에 만들 때 씀 (패널 아래 가운데 버튼)
    public void BuildUpgradeButton()
    {
        RectTransform parent = obj.transform as RectTransform;
        if (parent == null)
            return;
        var go = new GameObject("RuneUpgradeButton", typeof(RectTransform), typeof(Image), typeof(Button));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(Mathf.Max(320f, parent.rect.width * 0.6f), 80f);
        rt.anchoredPosition = new Vector2(0f, -10f);
        var img = go.GetComponent<Image>();
        Image bg = obj.GetComponent<Image>();
        if (bg != null && bg.sprite != null)
        {
            img.sprite = bg.sprite;
            img.type = bg.type;
        }
        img.color = new Color(0.35f, 0.25f, 0.6f, 1f);
        upgradeButton = go.GetComponent<Button>();
        upgradeButton.targetGraphic = img;

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        var trt = (RectTransform)textGo.transform;
        trt.SetParent(rt, false);
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(10f, 4f);
        trt.offsetMax = new Vector2(-10f, -4f);
        var t = textGo.GetComponent<TextMeshProUGUI>();
        TMP_Text src = descText != null ? descText : lvText;
        if (src != null)
        {
            t.font = src.font;
            t.fontSharedMaterial = src.fontSharedMaterial;
        }
        t.enableAutoSizing = true;
        t.fontSizeMin = 18f;
        t.fontSizeMax = 40f;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
        t.raycastTarget = false;
        upgradeText = t;
    }
    #endregion
}
