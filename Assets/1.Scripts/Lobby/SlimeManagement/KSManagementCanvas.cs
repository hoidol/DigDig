using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// 슬라임 관리: 상단 능력치(SlimeState) + 탭 2개
// (2026-11-26) 탭: [용사] = 보유 용사 3열 그리드(HeroCollectionContainer) / [수련] = 레벨 스탯(LevelStatContainer, 이전 화면 이름 "용사 강화")
//   씬의 탭 버튼(KSMMenuTapButton) 0 = 용사, 1 = 수련 (글씨는 실행 중 번역 key KSM_Tab_Hero / KSM_Tab_Training으로 바꿈)
//   용사를 고르면 슬라임 그림(SlimeBody) 자리에 용사 정보(그림·이름·설명) + 능력치 칸이 그 용사 Lv.1 스탯(+ 수련 포인트)으로, 다시 누르면 해제
//   용사 탭 창·용사 정보·탭 글씨는 씬에 만들어 둠 (메뉴 Tools/Lobby/Add Hero Tab To KSManagement, KSHeroTabBuilder). 실행 중 생성 없음 → 씬에서 직접 수정
public class KSManagementCanvas : BaseLobbyCanvas
{
    public KSStatStateContainer kSStatStateContainer;
    [FormerlySerializedAs("authorityContainer")][FormerlySerializedAs("kSEquipmentContainer")]
    public LevelStatContainer levelStatContainer;

    [Header("용사 탭 (2026-11-26, 씬에 만들어 연결: Tools/Lobby/Add Hero Tab To KSManagement)")]
    public HeroCollectionContainer heroContainer;
    public RectTransform heroPreview;      // 슬라임 자리에 보이는 용사 정보
    public Image heroPreviewImage;
    public TMP_Text heroPreviewName;
    public TMP_Text heroPreviewDesc;
    public GameObject slimeBody;       // 비우면 SlimeState 하위 "SlimeBody"

    KSMenuType currentMenu = KSMenuType.Hero;
    HeroDataTable.HeroData shownHero;

    // 용사 칸 바탕 그림 (수련 탭 칸과 같은 모양)
    public Image SlotStyleSource => levelStatContainer != null && levelStatContainer.slotPanels != null
        ? levelStatContainer.slotPanels.Where(p => p != null).Select(p => p.GetComponent<Image>()).FirstOrDefault(i => i != null) : null;

    void Awake()
    {
        if (slimeBody == null && kSStatStateContainer != null)
            slimeBody = kSStatStateContainer.transform.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "SlimeBody")?.gameObject;
        if (heroContainer != null)
            heroContainer.owner = this;
        else
            Debug.LogWarning("[KSManagementCanvas] 용사 탭(heroContainer) 연결 없음 → 메뉴 Tools/Lobby/Add Hero Tab To KSManagement");
    }

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);

        kSStatStateContainer.OpenContainer();
        levelStatContainer.OpenContainer();
        if (heroContainer != null)
            heroContainer.ClearSelection();
        ShowHero(null);
        SelectMenu(currentMenu);
        UpdateCanvas();
    }

    public void SelectMenu(KSMenuType menuType)
    {
        currentMenu = heroContainer != null ? menuType : KSMenuType.LevelStat;
        levelStatContainer.gameObject.SetActive(currentMenu == KSMenuType.LevelStat);
        if (heroContainer != null)
        {
            heroContainer.gameObject.SetActive(currentMenu == KSMenuType.Hero);
            if (currentMenu == KSMenuType.Hero)
                heroContainer.OpenContainer();
        }
        foreach (KSMMenuTapButton btn in GetComponentsInChildren<KSMMenuTapButton>(true))
        {
            Image img = btn.image != null ? btn.image : btn.GetComponent<Image>();
            if (img != null)
                img.color = btn.menuType == currentMenu ? ColorSetting.activeColor : ColorSetting.inactiveColor;
        }
    }

    public override void UpdateCanvas()
    {
        kSStatStateContainer.UpdatePanel(shownHero);
        levelStatContainer.UpdateContainer();
        if (heroContainer != null && heroContainer.gameObject.activeInHierarchy)
            heroContainer.Refresh();
    }

    // 용사 정보 보이기 (null = 슬라임)
    public void ShowHero(HeroDataTable.HeroData hero)
    {
        shownHero = hero;
        if (slimeBody != null)
            slimeBody.SetActive(hero == null);
        if (heroPreview != null)
            heroPreview.gameObject.SetActive(hero != null);
        if (hero != null)
        {
            AbilityData card = HeroCollectionContainer.GetHeroCard(hero.key);
            if (heroPreviewImage != null)
            {
                heroPreviewImage.sprite = card != null ? card.thum : null;
                heroPreviewImage.enabled = heroPreviewImage.sprite != null;
            }
            if (heroPreviewName != null)
                heroPreviewName.text = card != null && !string.IsNullOrEmpty(card.title) ? card.title : hero.key;
            if (heroPreviewDesc != null)
                heroPreviewDesc.text = hero.desc;
        }
        kSStatStateContainer.UpdatePanel(shownHero);
    }

#if UNITY_EDITOR
    // ---- 에디터 빌더(KSHeroTabBuilder)가 씬에 만들 때만 씀 ----
    // 탭 글씨: 0 = 용사, 1 = 수련 (TranslateText key를 씬에 저장)
    public void SetTabLabels()
    {
        foreach (KSMMenuTapButton btn in GetComponentsInChildren<KSMMenuTapButton>(true))
        {
            string key = btn.menuType == KSMenuType.Hero ? "KSM_Tab_Hero" : "KSM_Tab_Training";
            TranslateText tt = btn.GetComponentInChildren<TranslateText>(true);
            if (tt != null)
            {
                tt.key = key;
                tt.formatArg = null;
                tt.UpdatePanel();
            }
            else
            {
                TMP_Text label = btn.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                    label.text = TranslateManager.GetText(key);
            }
        }
    }

    // 용사 탭 창: 수련 탭 창(테두리 + 스크롤)을 복사해 내용만 비우고 3열 그리드로
    public void BuildHeroContainer()
    {
        if (levelStatContainer == null)
            return;
        GameObject src = levelStatContainer.gameObject;
        bool wasActive = src.activeSelf;
        src.SetActive(false); // 복사본의 Awake가 돌지 않게
        GameObject go = Instantiate(src, src.transform.parent);
        src.SetActive(wasActive);
        go.name = "HeroContainer";
        go.transform.SetSiblingIndex(src.transform.GetSiblingIndex());
        foreach (MonoBehaviour mb in go.GetComponents<MonoBehaviour>())
            if (mb is KSMContainer || mb is TapPanel)
                DestroyImmediate(mb);

        RectTransform content = go.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(t => t.name == "Contents");
        if (content == null)
        {
            Debug.LogWarning("[KSManagementCanvas] 수련 탭 창에 Contents가 없어 용사 탭을 못 만듦");
            DestroyImmediate(go);
            return;
        }
        for (int i = content.childCount - 1; i >= 0; i--)
            DestroyImmediate(content.GetChild(i).gameObject);
        foreach (LayoutGroup lg in content.GetComponents<LayoutGroup>())
            DestroyImmediate(lg);
        GridLayoutGroup grid = content.gameObject.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = HeroCollectionContainer.COLUMN_COUNT;
        grid.spacing = new Vector2(24f, 24f);
        grid.padding = new RectOffset(24, 24, 24, 24);
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.cellSize = new Vector2(280f, 336f);
        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        if (fitter == null)
            fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        heroContainer = go.AddComponent<HeroCollectionContainer>();
        heroContainer.content = content;
        heroContainer.owner = this;
        go.SetActive(false);
    }

    // 용사 정보: 슬라임 그림과 같은 자리·크기 (그림 + 이름 + 설명)
    public void BuildHeroPreview()
    {
        RectTransform body = slimeBody != null ? slimeBody.transform as RectTransform : null;
        RectTransform parent = body != null ? body.parent as RectTransform : kSStatStateContainer.transform as RectTransform;
        var go = new GameObject("HeroPreview", typeof(RectTransform));
        heroPreview = (RectTransform)go.transform;
        heroPreview.SetParent(parent, false);
        if (body != null)
        {
            heroPreview.anchorMin = body.anchorMin;
            heroPreview.anchorMax = body.anchorMax;
            heroPreview.pivot = body.pivot;
            heroPreview.anchoredPosition = body.anchoredPosition;
            heroPreview.sizeDelta = body.sizeDelta;
            heroPreview.SetSiblingIndex(body.GetSiblingIndex() + 1);
        }
        else
            heroPreview.sizeDelta = new Vector2(500f, 500f);

        var img = NewChild("Image", heroPreview, new Vector2(0.1f, 0.3f), new Vector2(0.9f, 1f));
        heroPreviewImage = img.gameObject.AddComponent<Image>();
        heroPreviewImage.preserveAspect = true;
        heroPreviewImage.raycastTarget = false;

        TMP_Text fontSrc = GetComponentInChildren<TMP_Text>(true);
        heroPreviewName = NewText("Name", heroPreview, new Vector2(0f, 0.17f), new Vector2(1f, 0.3f), 44f, new Color(1f, 0.85f, 0.35f), fontSrc);
        heroPreviewName.fontStyle = FontStyles.Bold;
        heroPreviewDesc = NewText("Desc", heroPreview, new Vector2(0f, 0f), new Vector2(1f, 0.17f), 28f, Color.white, fontSrc);
    }

    static RectTransform NewChild(string name, RectTransform parent, Vector2 min, Vector2 max)
    {
        var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    static TMP_Text NewText(string name, RectTransform parent, Vector2 min, Vector2 max, float size, Color color, TMP_Text fontSrc)
    {
        var t = NewChild(name, parent, min, max).gameObject.AddComponent<TextMeshProUGUI>();
        if (fontSrc != null)
        {
            t.font = fontSrc.font;
            t.fontSharedMaterial = fontSrc.fontSharedMaterial;
        }
        t.fontSize = size;
        t.enableAutoSizing = true;
        t.fontSizeMin = 16f;
        t.fontSizeMax = size;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }
#endif
}
