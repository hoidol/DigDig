using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// (2026-11-24) wave 중 하단 UI (브레이크타임엔 카드 줄 OfferButtons가 그 자리. BottomGameCanvas가 켜고 끔)
//   1. 킹 체력 = Slider (경험치바 ExpSlider와 같은 모양, 킹 머리 위 체력 표시는 숨김)
//   2. 에너지: 용사마다 이름·아이콘·색 (투지·집중·어둠). 개수는 숫자만 "1/2" + 다음 1개 충전 Slider + 남은 초
//      최대 개수 = 2개 + 용사 강화 카드 "에너지 보유 증가" (HeroKingAbility.MaxCharges)
//   3. 스킬 버튼 3칸: 누르면 필요한 에너지를 쓰고 발동 (HeroKingAbility.TryUseSkill)
//      소모량 = 에너지 아이콘을 필요한 개수만큼 (costContainer 아래 costIconTemplate을 복사, 모자라면 흐리게)
//      아직 해방 레벨(Lv.2·5·8)에 못 미친 칸 = 자물쇠 아이콘 + 필요 레벨 "Lv.N" (lockObject·lockLevelText)
//   (2026-11-25) 스킬 칸에 마우스를 올리거나 손가락으로 길게(tooltipHoldTime) 누르면 위에 설명 말풍선 (이름·소모 에너지·설명·해방 레벨)
//      길게 눌렀다 떼면 스킬은 안 씀. 말풍선 = tooltipRoot (비우면 실행 중 기본 모양 생성)
// 씬: InGame BottomGameCanvas/Panel/HeroSkillHud (메뉴 Tools/Hero/Add HeroSkillHud To InGame Scene으로 생성, 모양·위치는 씬에서 직접 수정)
//   아래 연결 칸에 씬 오브젝트를 넣어 두면 그걸 갱신만 함. 비어 있는 칸은 무시 (원하는 것만 남기고 지워도 됨)
//   씬에 없으면 실행 중 OfferButtons 자리에 기본 모양 생성 (Create)
public class HeroSkillHud : MonoBehaviour
{
    public const float SIDE = 30f, HP_H = 64f, ENERGY_H = 64f, SKILL_H = 210f, GAP = 14f;

    [Header("킹 체력")]
    public Slider hpSlider;       // 0~1 = 체력 비율
    public TMP_Text hpText;       // "12 / 20"
    [HideInInspector] public Image hpFill; // (이전 방식, 연결돼 있으면 같이 갱신)

    [Header("에너지 (용사 색으로 칠함)")]
    public Image energyIcon;      // 용사 energyIcon, 없으면 용사 색 사각형
    public TMP_Text energyNameText;
    public TMP_Text energyCountText; // "1/2"
    [HideInInspector] public TMP_Text energyTimeText; // (2026-11-25 삭제) 남은 초 글씨. 씬에 연결돼 있으면 Awake에서 숨김
    public Slider energySlider;   // 다음 1개 충전 진행도 (채움 색 = 용사 색)
    [HideInInspector] public Image energyFill; // (이전 방식)

    [Header("스킬 버튼 (0 = 1단계, 1 = 2단계, 2 = 공용)")]
    public HeroSkillSlot[] slots = new HeroSkillSlot[HeroKingAbility.SKILL_COUNT];
    [Tooltip("켜면 해방된 스킬 칸 바탕 = 용사 색 × slotColorMul. 끄면 씬에서 정한 바탕색 그대로")]
    public bool tintSlotWithHeroColor = true;
    public Color slotColorMul = new Color(0.55f, 0.55f, 0.55f, 1f);
    public Color lockedSlotColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    [Tooltip("잠긴 칸의 스킬 이름 글씨 투명도 (자물쇠·필요 레벨은 그대로 선명하게)")]
    [Range(0f, 1f)] public float lockedAlpha = 0.45f;
    [Tooltip("에너지가 모자랄 때 칸 투명도")]
    [Range(0f, 1f)] public float noEnergyAlpha = 0.65f;
    [Tooltip("소모 아이콘 중 아직 모자란 개수만큼 흐리게")]
    [Range(0f, 1f)] public float missingCostAlpha = 0.3f;

    [Header("스킬 설명 말풍선 (비우면 실행 중 생성)")]
    public RectTransform tooltipRoot;
    public TMP_Text tooltipTitleText;
    public TMP_Text tooltipDescText;
    [Tooltip("손가락으로 이만큼(초, 실제 시간) 누르고 있으면 설명이 보임")]
    public float tooltipHoldTime = 0.35f;
    public float tooltipGap = 16f; // 스킬 칸 위 간격

    [System.Serializable]
    public class HeroSkillSlot
    {
        public Button button;
        public Image bg;
        public Image icon;
        public TMP_Text nameText;
        public RectTransform costContainer; // 소모 에너지 아이콘 줄
        public Image costIconTemplate;      // 이걸 복사해 필요한 개수만큼 (용사 energyIcon, 없으면 용사 색)
        public CanvasGroup group;   // 에너지 부족 흐리게
        public GameObject lockObject; // 잠김 표시 (자물쇠 아이콘 + 필요 레벨). 해방되면 숨김
        public TMP_Text lockLevelText; // "Lv.2"
        [HideInInspector] public TMP_Text infoText; // (이전 방식 "투지 1", 연결돼 있으면 숨김)
        [HideInInspector] public Color baseColor;
        [System.NonSerialized] public System.Collections.Generic.List<Image> costIcons;
    }

    Sprite whiteSprite;

    void Awake()
    {
        if (energyTimeText != null) // (2026-11-25) 충전까지 남은 시간 글씨 삭제 (씬 오브젝트가 남아 있어도 숨김)
            energyTimeText.gameObject.SetActive(false);
        for (int i = 0; i < slots.Length; i++)
        {
            HeroSkillSlot slot = slots[i];
            if (slot == null)
                continue;
            if (slot.bg != null)
                slot.baseColor = slot.bg.color;
            int idx = i;
            if (slot.button != null)
            {
                slot.button.onClick.AddListener(() => OnClickedSkill(idx));
                HeroSkillTooltipTrigger trigger = slot.button.GetComponent<HeroSkillTooltipTrigger>()
                    ?? slot.button.gameObject.AddComponent<HeroSkillTooltipTrigger>();
                trigger.Init(this, idx);
            }
        }
    }

    #region 스킬 설명 말풍선 (2026-11-25)
    int tooltipIdx = -1;

    public void ShowTooltip(int idx)
    {
        if (slots == null || idx < 0 || idx >= slots.Length || slots[idx] == null)
            return;
        EnsureTooltip();
        if (tooltipRoot == null)
            return;
        tooltipIdx = idx;
        RefreshTooltipText();
        tooltipRoot.gameObject.SetActive(true);
        tooltipRoot.SetAsLastSibling();
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRoot);
        PlaceTooltip();
    }

    public void HideTooltip(int idx)
    {
        if (idx >= 0 && idx != tooltipIdx)
            return;
        tooltipIdx = -1;
        if (tooltipRoot != null)
            tooltipRoot.gameObject.SetActive(false);
    }

    void RefreshTooltipText()
    {
        HeroKingAbility hero = Hero;
        int i = tooltipIdx;
        string title, desc;
        if (hero == null)
        {
            title = i == HeroKingAbility.COMMON_SKILL_INDEX ? HeroKingAbility.COMMON_SKILL_NAME : "스킬";
            desc = $"Lv.{LevelUpChoiceRule.HERO_LEVEL}에 용사를 고르면 스킬이 정해집니다.";
        }
        else
        {
            title = $"{hero.GetSkillName(i)}  <size=80%><color=#{ColorUtility.ToHtmlStringRGB(hero.energyColor)}>{hero.energyName} {hero.GetSkillCost(i)}</color></size>";
            desc = hero.GetSkillDesc(i);
            if (!hero.IsSkillUnlocked(i))
                desc += $"\n<color=#FFD95A>Lv.{LevelUpChoiceRule.GetSkillUnlockLevel(i)}에 해방</color>";
        }
        if (tooltipTitleText != null)
            tooltipTitleText.text = title;
        if (tooltipDescText != null)
            tooltipDescText.text = desc;
    }

    // 누른 칸 위 가운데, HUD 가로 범위 밖으로 안 나가게
    void PlaceTooltip()
    {
        RectTransform slotRt = slots[tooltipIdx].bg != null ? slots[tooltipIdx].bg.rectTransform
            : slots[tooltipIdx].button != null ? (RectTransform)slots[tooltipIdx].button.transform : null;
        var hudRt = (RectTransform)transform;
        if (slotRt == null)
            return;
        var corners = new Vector3[4];
        slotRt.GetWorldCorners(corners); // 0 왼아래, 1 왼위, 2 오른위
        Vector3 topCenter = (corners[1] + corners[2]) * 0.5f;
        Vector2 local = hudRt.InverseTransformPoint(topCenter);
        tooltipRoot.anchorMin = tooltipRoot.anchorMax = new Vector2(0.5f, 0.5f);
        tooltipRoot.pivot = new Vector2(0.5f, 0f);
        float half = tooltipRoot.rect.width * 0.5f;
        Rect hud = hudRt.rect;
        float x = Mathf.Clamp(local.x, hud.xMin + half, hud.xMax - half);
        tooltipRoot.anchoredPosition = new Vector2(x - hud.center.x, local.y - hud.center.y + tooltipGap);
    }

    void EnsureTooltip()
    {
        if (tooltipRoot != null)
            return;
        TMP_Text src = GetComponentInChildren<TMP_Text>(true);
        var go = new GameObject("SkillTooltip", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        tooltipRoot = (RectTransform)go.transform;
        tooltipRoot.SetParent(transform, false);
        tooltipRoot.sizeDelta = new Vector2(560f, 0f);
        var bg = go.GetComponent<Image>();
        bg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);
        bg.raycastTarget = false;
        var layout = go.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 18, 20);
        layout.spacing = 8f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        var fitter = go.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        tooltipTitleText = MakeTooltipText("Title", 36, FontStyles.Bold, src);
        tooltipDescText = MakeTooltipText("Desc", 30, FontStyles.Normal, src);
        go.SetActive(false);
    }

    TMP_Text MakeTooltipText(string name, float size, FontStyles style, TMP_Text src)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(tooltipRoot, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (src != null)
            t.font = src.font;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = Color.white;
        t.alignment = TextAlignmentOptions.TopLeft;
        t.enableWordWrapping = true;
        t.raycastTarget = false;
        return t;
    }
    #endregion

    #region 기본 모양 만들기 (실행 중 / 에디터 메뉴 공용)
    // 실행 중: 씬에 없을 때 OfferButtons 자리에 생성 (Slider 모양 = OfferButtons 안 ExpSlider)
    public static HeroSkillHud Create(RectTransform reference)
    {
        if (reference == null || reference.parent == null)
            return null;
        var white = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
        HeroSkillHud hud = Build(reference, white, FindSprite(reference.root, "Button_01_White_Bg"), FindSprite(reference.root, "Lock"),
            null, reference.GetComponentInChildren<Slider>(true));
        hud.whiteSprite = white;
        HideKingHeadHp();
        hud.gameObject.SetActive(false);
        return hud;
    }

    public static Sprite FindSprite(Transform root, string contains)
    {
        foreach (var img in root.GetComponentsInChildren<Image>(true))
            if (img.sprite != null && img.sprite.name.Contains(contains))
                return img.sprite;
        return null;
    }

    // reference(OfferButtons) 옆에 HUD 오브젝트를 만들고 연결 칸을 채움
    // plain = 기본 칸 그림, box = 스킬 버튼 바탕, costSprite = 소모 아이콘 기본 그림, sliderTemplate = 복사할 Slider (없으면 기본 Slider)
    public static HeroSkillHud Build(RectTransform reference, Sprite plain, Sprite box, Sprite lockSprite = null, Sprite costSprite = null, Slider sliderTemplate = null)
    {
        var go = new GameObject("HeroSkillHud", typeof(RectTransform));
        var root = (RectTransform)go.transform;
        root.SetParent(reference.parent, false);
        root.SetSiblingIndex(reference.GetSiblingIndex() + 1);
        root.anchorMin = new Vector2(0f, 0f);
        root.anchorMax = new Vector2(1f, 0f);
        root.pivot = new Vector2(0.5f, 0f);
        root.sizeDelta = new Vector2(-SIDE * 2f, HP_H + ENERGY_H + SKILL_H + GAP * 2f);
        root.anchoredPosition = new Vector2(0f, 40f);
        var hud = go.AddComponent<HeroSkillHud>();
        Builder b = MakeBuilder(reference, plain, box, lockSprite, costSprite, sliderTemplate);

        // 아래부터: 스킬 → 에너지 → 체력
        float y = 0f;
        var skillRow = Builder.Row("SkillRow", root, y, SKILL_H);
        y += SKILL_H + GAP;
        var energyRow = Builder.Row("EnergyRow", root, y, ENERGY_H);
        y += ENERGY_H + GAP;
        var hpRow = Builder.Row("HpRow", root, y, HP_H);

        // 체력
        hud.hpSlider = b.HpSlider(hpRow);
        hud.hpText = b.Text("HpText", hpRow, 40, TextAlignmentOptions.Center, "20 / 20");

        // 에너지: [아이콘][이름][1/2][충전 Slider]
        b.Box("Bg", energyRow, new Color(0f, 0f, 0f, 0.6f));
        hud.energyIcon = b.Image("Icon", energyRow);
        Builder.Place(hud.energyIcon.rectTransform, 8f, 56f, 56f);
        hud.energyNameText = b.Text("Name", energyRow, 36, TextAlignmentOptions.MidlineLeft, "에너지");
        Builder.Place(hud.energyNameText.rectTransform, 72f, 170f, ENERGY_H);
        hud.energyCountText = b.Text("Count", energyRow, 36, TextAlignmentOptions.MidlineLeft, "0/0");
        Builder.Place(hud.energyCountText.rectTransform, 250f, 100f, ENERGY_H);
        hud.energySlider = b.EnergySlider(energyRow);

        // 스킬 3칸
        var layout = skillRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 16f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = true;
        hud.slots = new HeroSkillSlot[HeroKingAbility.SKILL_COUNT];
        for (int i = 0; i < hud.slots.Length; i++)
            hud.slots[i] = b.Slot(skillRow, i);
        return hud;
    }

    static Builder MakeBuilder(Transform reference, Sprite plain, Sprite box, Sprite lockSprite, Sprite costSprite, Slider sliderTemplate)
    {
        TMP_Text src = reference.GetComponentInChildren<TMP_Text>(true) ?? reference.root.GetComponentInChildren<TMP_Text>(true);
        return new Builder
        {
            font = src != null ? src.font : null,
            plain = plain,
            box = box,
            lockSprite = lockSprite,
            costSprite = costSprite != null ? costSprite : plain,
            sliderTemplate = sliderTemplate,
        };
    }

    // 이미 만든 스킬 칸에 잠김 표시(자물쇠 + 필요 레벨)만 추가 (씬에 만든 HUD에 나중에 붙일 때, 에디터 메뉴)
    public static void AddLock(HeroSkillSlot slot, int idx, Sprite lockSprite, TMP_FontAsset font)
    {
        if (slot == null || slot.bg == null)
            return;
        new Builder { font = font, lockSprite = lockSprite }.Lock(slot, slot.bg.rectTransform, idx);
    }

    // (2026-11-24) 씬에 만든 이전 HUD를 바꿈: 체력·충전 바 → Slider, 에너지 개수 칸(OOO) 삭제, 스킬 칸 "투지 1" 글씨 → 소모 아이콘 줄
    // 이미 바뀐 부분은 건너뜀. 나머지(위치·글씨 크기 등 직접 고친 것)는 그대로
    public static int Upgrade(HeroSkillHud hud, Sprite plain, Sprite costSprite, Slider sliderTemplate)
    {
        Builder b = MakeBuilder(hud.transform, plain, null, null, costSprite, sliderTemplate);
        int changed = 0;

        if (hud.hpSlider == null)
        {
            Transform row = hud.hpFill != null ? hud.hpFill.transform.parent : hud.hpText != null ? hud.hpText.transform.parent : null;
            if (row != null)
            {
                for (int i = row.childCount - 1; i >= 0; i--) // 이전 바탕·채움 이미지 삭제 (글씨는 남김)
                {
                    Transform c = row.GetChild(i);
                    if (c.GetComponent<TMP_Text>() == null)
                        DestroyNow(c.gameObject);
                }
                hud.hpSlider = b.HpSlider((RectTransform)row);
                hud.hpSlider.transform.SetAsFirstSibling();
                hud.hpFill = null;
                changed++;
            }
        }

        Transform energyRow = hud.energyCountText != null ? hud.energyCountText.transform.parent : null;
        if (energyRow != null)
        {
            for (int i = energyRow.childCount - 1; i >= 0; i--) // 개수 칸(Pip0~2) 삭제
            {
                Transform c = energyRow.GetChild(i);
                if (c.name.StartsWith("Pip"))
                {
                    DestroyNow(c.gameObject);
                    changed++;
                }
            }
            if (hud.energySlider == null)
            {
                Transform oldBar = hud.energyFill != null ? hud.energyFill.transform.parent : energyRow.Find("ChargeBg");
                if (oldBar != null && oldBar != energyRow)
                    DestroyNow(oldBar.gameObject);
                hud.energySlider = b.EnergySlider((RectTransform)energyRow);
                hud.energySlider.transform.SetSiblingIndex(hud.energyCountText.transform.GetSiblingIndex() + 1);
                hud.energyFill = null;
                Builder.Place(hud.energyCountText.rectTransform, 250f, 100f, ENERGY_H);
                changed++;
            }
        }

        for (int i = 0; i < hud.slots.Length; i++)
        {
            HeroSkillSlot slot = hud.slots[i];
            if (slot == null || slot.bg == null)
                continue;
            if (slot.infoText != null)
            {
                DestroyNow(slot.infoText.gameObject);
                slot.infoText = null;
                changed++;
            }
            if (slot.costContainer == null)
            {
                b.Cost(slot, slot.bg.rectTransform);
                if (slot.lockObject != null)
                    slot.lockObject.transform.SetAsLastSibling(); // 자물쇠가 위에
                changed++;
            }
        }
        return changed;
    }

    static void DestroyNow(Object o)
    {
        if (Application.isPlaying)
            Destroy(o);
        else
            DestroyImmediate(o);
    }

    // UI 오브젝트 만드는 도우미
    class Builder
    {
        public TMP_FontAsset font;
        public Sprite plain, box, lockSprite, costSprite;
        public Slider sliderTemplate;

        public static RectTransform Row(string name, RectTransform parent, float bottom, float height)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(0f, height);
            rt.anchoredPosition = new Vector2(0f, bottom);
            return rt;
        }

        public Image Box(string name, RectTransform parent, Color color, float inset = 0f)
        {
            var img = Image(name, parent);
            Stretch(img.rectTransform, inset);
            img.color = color;
            return img;
        }

        public Image Image(string name, RectTransform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = plain;
            img.raycastTarget = false;
            return img;
        }

        public static void Stretch(RectTransform rt, float inset)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(inset, inset);
            rt.offsetMax = new Vector2(-inset, -inset);
        }

        // 왼쪽 기준 x에 가로 w, 세로 h (세로 가운데)
        public static void Place(RectTransform rt, float x, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(x, 0f);
        }

        public TMP_Text Text(string name, RectTransform parent, float size, TextAlignmentOptions align, string text)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            Stretch(rt, 0f);
            var t = go.GetComponent<TextMeshProUGUI>();
            if (font != null)
                t.font = font;
            t.fontSize = size;
            t.color = Color.white;
            t.alignment = align;
            t.raycastTarget = false;
            t.text = text;
            return t;
        }

        // 체력 Slider: 줄 전체, 채움 = 빨강
        public Slider HpSlider(RectTransform row)
        {
            Slider s = MakeSlider("HpSlider", row, new Color(0.9f, 0.25f, 0.3f));
            Stretch((RectTransform)s.transform, 0f);
            return s;
        }

        // 에너지 충전 Slider: 개수 글씨 오른쪽 ~ 남은 초 왼쪽
        public Slider EnergySlider(RectTransform row)
        {
            Slider s = MakeSlider("ChargeSlider", row, new Color(0.55f, 0.85f, 1f));
            var rt = (RectTransform)s.transform;
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.offsetMin = new Vector2(360f, -16f);
            rt.offsetMax = new Vector2(-130f, 16f);
            return s;
        }

        // sliderTemplate(경험치바 ExpSlider)를 복사해 글씨는 빼고 쓰기 전용으로. 템플릿이 없으면 기본 Slider
        Slider MakeSlider(string name, RectTransform parent, Color fillColor)
        {
            Slider s;
            if (sliderTemplate != null)
            {
                s = Object.Instantiate(sliderTemplate, parent, false);
                foreach (var t in s.GetComponentsInChildren<TMP_Text>(true))
                    DestroyNow(t.gameObject);
                foreach (var mb in s.GetComponents<MonoBehaviour>())
                    if (mb != s)
                        DestroyNow(mb);
            }
            else
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
                go.transform.SetParent(parent, false);
                s = go.GetComponent<Slider>();
                var bg = Image("Background", (RectTransform)go.transform);
                Stretch(bg.rectTransform, 0f);
                bg.color = new Color(0f, 0f, 0f, 0.6f);
                var area = new GameObject("Fill Area", typeof(RectTransform));
                var areaRt = (RectTransform)area.transform;
                areaRt.SetParent(go.transform, false);
                Stretch(areaRt, 4f);
                var fill = Image("Fill", areaRt);
                fill.rectTransform.anchorMin = Vector2.zero;
                fill.rectTransform.anchorMax = new Vector2(0f, 1f);
                fill.rectTransform.sizeDelta = Vector2.zero;
                s.fillRect = fill.rectTransform;
            }
            s.name = name;
            s.interactable = false;
            s.transition = Selectable.Transition.None;
            s.navigation = new Navigation { mode = Navigation.Mode.None };
            s.minValue = 0f;
            s.maxValue = 1f;
            s.wholeNumbers = false;
            s.value = 1f;
            if (s.fillRect != null && s.fillRect.TryGetComponent(out Image fillImg))
                fillImg.color = fillColor;
            foreach (var g in s.GetComponentsInChildren<Graphic>(true))
                g.raycastTarget = false;
            return s;
        }

        public HeroSkillSlot Slot(RectTransform parent, int idx)
        {
            var go = new GameObject($"Skill{idx}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(CanvasGroup));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            var slot = new HeroSkillSlot
            {
                bg = go.GetComponent<Image>(),
                button = go.GetComponent<Button>(),
                group = go.GetComponent<CanvasGroup>(),
            };
            slot.bg.sprite = box != null ? box : plain;
            slot.bg.type = box != null ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
            slot.bg.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            slot.button.targetGraphic = slot.bg;

            slot.icon = Image("Icon", rt);
            var iRt = slot.icon.rectTransform;
            iRt.anchorMin = iRt.anchorMax = new Vector2(0.5f, 1f);
            iRt.pivot = new Vector2(0.5f, 1f);
            iRt.sizeDelta = new Vector2(90f, 90f);
            iRt.anchoredPosition = new Vector2(0f, -12f);
            slot.icon.preserveAspect = true;

            slot.nameText = Text("Name", rt, 34, TextAlignmentOptions.Center, idx == HeroKingAbility.COMMON_SKILL_INDEX ? HeroKingAbility.COMMON_SKILL_NAME : $"스킬 {idx + 1}");
            var nRt = slot.nameText.rectTransform;
            nRt.anchorMin = new Vector2(0f, 0f);
            nRt.anchorMax = new Vector2(1f, 0f);
            nRt.pivot = new Vector2(0.5f, 0f);
            nRt.sizeDelta = new Vector2(-10f, 50f);
            nRt.anchoredPosition = new Vector2(0f, 46f);
            slot.nameText.enableAutoSizing = true;
            slot.nameText.fontSizeMin = 20;
            slot.nameText.fontSizeMax = 34;

            Cost(slot, rt);
            Lock(slot, rt, idx);
            return slot;
        }

        // 소모 에너지 아이콘 줄 (칸 아래쪽 가운데). 템플릿 아이콘 하나만 만들어 두고 실행 중 필요한 개수만큼 복사
        public void Cost(HeroSkillSlot slot, RectTransform parent)
        {
            var go = new GameObject("Cost", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(-10f, 36f);
            rt.anchoredPosition = new Vector2(0f, 10f);
            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 6f;
            layout.childControlWidth = layout.childControlHeight = false;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            var icon = Image("CostIcon", rt);
            icon.sprite = costSprite;
            icon.preserveAspect = true;
            icon.rectTransform.sizeDelta = new Vector2(32f, 32f);
            icon.gameObject.SetActive(false);
            slot.costContainer = rt;
            slot.costIconTemplate = icon;
        }

        // 잠김 표시: 스킬 아이콘 자리에 자물쇠, 아래 글씨 자리에 "Lv.N"
        public void Lock(HeroSkillSlot slot, RectTransform parent, int idx)
        {
            var go = new GameObject("Lock", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            Stretch(rt, 0f);
            var icon = Image("LockIcon", rt);
            icon.sprite = lockSprite != null ? lockSprite : plain;
            icon.preserveAspect = true;
            var iRt = icon.rectTransform;
            iRt.anchorMin = iRt.anchorMax = new Vector2(0.5f, 1f);
            iRt.pivot = new Vector2(0.5f, 1f);
            iRt.sizeDelta = new Vector2(80f, 80f);
            iRt.anchoredPosition = new Vector2(0f, -16f);
            var t = Text("LockLevel", rt, 34, TextAlignmentOptions.Center, $"Lv.{LevelUpChoiceRule.GetSkillUnlockLevel(idx)}");
            t.color = new Color(1f, 0.85f, 0.35f);
            t.fontStyle = FontStyles.Bold;
            var tRt = t.rectTransform;
            tRt.anchorMin = new Vector2(0f, 0f);
            tRt.anchorMax = new Vector2(1f, 0f);
            tRt.pivot = new Vector2(0.5f, 0f);
            tRt.sizeDelta = new Vector2(-10f, 44f);
            tRt.anchoredPosition = new Vector2(0f, 6f);
            slot.lockObject = go;
            slot.lockLevelText = t;
        }
    }
    #endregion

    void OnEnable()
    {
        HideKingHeadHp();
        Refresh();
    }

    void OnDisable()
    {
        HideTooltip(-1);
        CancelAim();
    }

    void Update()
    {
        Refresh();
        UpdateAim();
        if (tooltipIdx >= 0 && tooltipRoot != null && tooltipRoot.gameObject.activeSelf)
            RefreshTooltipText(); // 에너지·해방 상태가 바뀌어도 맞게
    }

    // 킹 머리 위 체력 표시는 숨김 (체력은 여기서만)
    public static void HideKingHeadHp()
    {
        KingSlime king = KingSlime.Instance;
        if (king != null && king.health != null && king.health.healthCanvas != null)
            king.health.healthCanvas.gameObject.SetActive(false);
    }

    HeroKingAbility Hero => KingSlime.Instance != null && KingSlime.Instance.abilityInventory != null
        ? KingSlime.Instance.abilityInventory.Get<HeroKingAbility>() : null;

    static void SetSlider(Slider s, float ratio)
    {
        if (s == null)
            return;
        s.minValue = 0f;
        s.maxValue = 1f;
        s.SetValueWithoutNotify(Mathf.Clamp01(ratio));
    }

    void Refresh()
    {
        KingSlime king = KingSlime.Instance;
        if (king == null || king.statMgr == null)
            return;

        float maxHp = Mathf.Max(1f, king.MaxHp);
        float hpRatio = Mathf.Clamp01(king.CurHp / maxHp);
        SetSlider(hpSlider, hpRatio);
        if (hpFill != null)
            hpFill.fillAmount = hpRatio;
        if (hpText != null)
            hpText.text = $"{Mathf.CeilToInt(king.CurHp)} / {Mathf.CeilToInt(maxHp)}";

        HeroKingAbility hero = Hero;
        Color col = hero != null ? hero.energyColor : new Color(0.6f, 0.6f, 0.6f);
        col.a = 1f;
        bool hasIcon = hero != null && hero.energyIcon != null;
        if (energyIcon != null)
        {
            if (hasIcon)
                energyIcon.sprite = hero.energyIcon;
            else if (energyIcon.sprite == null && whiteSprite != null)
                energyIcon.sprite = whiteSprite;
            energyIcon.color = hasIcon ? Color.white : col;
        }
        if (energyNameText != null)
        {
            energyNameText.text = hero != null ? hero.energyName : "에너지";
            energyNameText.color = col;
        }
        int max = hero != null ? hero.MaxCharges : 0;
        int cur = hero != null ? hero.Charges : 0;
        string count, time;
        float fill;
        if (hero == null)
        {
            count = "";
            fill = 0f;
            time = $"Lv.{LevelUpChoiceRule.HERO_LEVEL} 용사";
        }
        else if (max == 0)
        {
            count = "0/0";
            fill = 0f;
            time = $"Lv.{LevelUpChoiceRule.GetSkillUnlockLevel(0)} 해방";
        }
        else
        {
            count = $"{cur}/{max}";
            fill = hero.IsFull ? 1f : hero.ChargeRatio;
            time = hero.IsFull ? "MAX" : $"{Mathf.CeilToInt(hero.ChargeTime - hero.ChargeProgress)}s";
        }
        if (energyCountText != null)
            energyCountText.text = count;
        SetSlider(energySlider, fill);
        if (energySlider != null && energySlider.fillRect != null && energySlider.fillRect.TryGetComponent(out Image eFill))
            eFill.color = col;
        if (energyFill != null)
        {
            energyFill.fillAmount = fill;
            energyFill.color = col;
        }
        if (energyTimeText != null)
            energyTimeText.text = time;

        if (slots == null)
            return;
        for (int i = 0; i < slots.Length; i++)
        {
            HeroSkillSlot slot = slots[i];
            if (slot == null)
                continue;
            bool unlocked = hero != null && hero.IsSkillUnlocked(i);
            bool showLock = !unlocked && slot.lockObject != null;
            int cost = hero != null ? hero.GetSkillCost(i) : 1;
            if (slot.lockObject != null)
                slot.lockObject.SetActive(!unlocked);
            if (slot.lockLevelText != null)
                slot.lockLevelText.text = $"Lv.{LevelUpChoiceRule.GetSkillUnlockLevel(i)}";
            if (slot.nameText != null)
            {
                slot.nameText.text = hero != null ? hero.GetSkillName(i) : i == HeroKingAbility.COMMON_SKILL_INDEX ? HeroKingAbility.COMMON_SKILL_NAME : "스킬";
                Color nc = slot.nameText.color;
                nc.a = unlocked ? 1f : lockedAlpha;
                slot.nameText.color = nc;
            }
            if (slot.icon != null)
            {
                Sprite icon = hero != null && !showLock ? hero.GetSkillIcon(i) : null;
                slot.icon.sprite = icon;
                slot.icon.enabled = icon != null;
            }
            if (slot.bg != null)
            {
                Color bg = tintSlotWithHeroColor ? col * slotColorMul : slot.baseColor;
                bg.a = 1f;
                slot.bg.color = unlocked ? bg : lockedSlotColor;
            }
            if (slot.infoText != null)
                slot.infoText.gameObject.SetActive(false);
            RefreshCost(slot, unlocked && !showLock ? cost : 0, cur, hasIcon ? hero.energyIcon : null, col);
            if (slot.group != null)
                slot.group.alpha = !unlocked || cur >= cost ? 1f : noEnergyAlpha;
        }
    }

    // 소모 아이콘을 cost개 보이게 (가진 에너지보다 많은 칸은 흐리게)
    void RefreshCost(HeroSkillSlot slot, int cost, int have, Sprite icon, Color color)
    {
        if (slot.costContainer == null || slot.costIconTemplate == null)
            return;
        slot.costContainer.gameObject.SetActive(cost > 0);
        if (slot.costIcons == null)
            slot.costIcons = new System.Collections.Generic.List<Image>();
        while (slot.costIcons.Count < cost)
        {
            Image copy = Instantiate(slot.costIconTemplate, slot.costContainer);
            copy.name = $"CostIcon{slot.costIcons.Count}";
            slot.costIcons.Add(copy);
        }
        for (int i = 0; i < slot.costIcons.Count; i++)
        {
            Image img = slot.costIcons[i];
            bool show = i < cost;
            img.gameObject.SetActive(show);
            if (!show)
                continue;
            if (icon != null)
                img.sprite = icon;
            Color c = icon != null ? Color.white : color;
            c.a = i < have ? 1f : missingCostAlpha;
            img.color = c;
        }
    }

    void OnClickedSkill(int idx)
    {
        // 길게 눌러 설명을 본 뒤 손을 떼면 스킬은 안 씀
        if (slots[idx] != null && slots[idx].button != null
            && slots[idx].button.TryGetComponent(out HeroSkillTooltipTrigger trigger) && trigger.ConsumeLongPress())
            return;
        HeroKingAbility hero = Hero;
        if (hero == null)
        {
            ToastCanvas.Toast($"Lv.{LevelUpChoiceRule.HERO_LEVEL}에 용사를 선택하세요");
            return;
        }
        // 조준 중인 칸을 다시 누르면 취소
        if (aimIdx == idx)
        {
            CancelAim();
            ToastCanvas.Toast("스킬 사용을 취소했습니다");
            return;
        }
        CancelAim();
        HandleResult(hero, idx, hero.TryUseSkill(idx));
    }

    void HandleResult(HeroKingAbility hero, int idx, HeroKingAbility.UseResult result)
    {
        switch (result)
        {
            case HeroKingAbility.UseResult.Used:
                SoundManager.Play(SFXType.PowerUp);
                if (slots[idx] != null && slots[idx].bg != null)
                {
                    RectTransform rt = slots[idx].bg.rectTransform;
                    DG.Tweening.DOTween.Kill(rt);
                    rt.localScale = Vector3.one * 1.1f;
                    DG.Tweening.ShortcutExtensions.DOScale(rt, 1f, 0.2f);
                }
                break;
            case HeroKingAbility.UseResult.NeedPosition:
                StartAim(hero, idx);
                break;
            case HeroKingAbility.UseResult.Locked:
                ToastCanvas.Toast($"Lv.{LevelUpChoiceRule.GetSkillUnlockLevel(idx)}에 해방됩니다");
                break;
            case HeroKingAbility.UseResult.NoEnergy:
                ToastCanvas.Toast($"{hero.energyName}이(가) 부족합니다 (필요 {hero.GetSkillCost(idx)})");
                break;
            case HeroKingAbility.UseResult.NoTarget:
                ToastCanvas.Toast("대상이 없습니다");
                break;
            case HeroKingAbility.UseResult.NotNow:
                ToastCanvas.Toast("wave 중에만 사용할 수 있습니다");
                break;
        }
    }

    #region 위치 지정 스킬 조준 (2026-11-25)
    // 버튼 → "원하는 위치를 누르세요" → 필드를 누르면 그 자리에 발동 (MinionFieldController.BeginPositionPick)
    // 조준 중: 그 칸이 깜빡이고, 마우스 위치에 범위 원 (손가락은 누르는 순간 발동). 같은 칸을 다시 누르면 취소
    int aimIdx = -1;
    LineRenderer aimRing;
    public bool IsAiming => aimIdx >= 0;

    void StartAim(HeroKingAbility hero, int idx)
    {
        MinionFieldController field = MinionFieldController.Instance;
        if (field == null)
            return;
        aimIdx = idx;
        field.BeginPositionPick(pos => OnAimPicked(idx, pos));
        ToastCanvas.Toast("원하는 위치를 누르세요");
        float radius = hero.GetTargetRadius(idx);
        if (radius > 0f)
        {
            Color c = hero.energyColor;
            c.a = 0.8f;
            aimRing = HeroSkillVisual.CreateLine("HeroAimRing", c, 0.06f, true);
            aimRing.gameObject.SetActive(false);
        }
    }

    void OnAimPicked(int idx, Vector2 pos)
    {
        ClearAimVisual();
        aimIdx = -1;
        HeroKingAbility hero = Hero;
        if (hero != null)
            HandleResult(hero, idx, hero.TryUseSkillAt(idx, pos));
    }

    public void CancelAim()
    {
        if (aimIdx >= 0 && MinionFieldController.Instance != null)
            MinionFieldController.Instance.CancelPositionPick();
        aimIdx = -1;
        ClearAimVisual();
    }

    void ClearAimVisual()
    {
        if (aimRing != null)
            Destroy(aimRing.gameObject);
        aimRing = null;
        if (slots != null)
            foreach (var slot in slots)
                if (slot != null && slot.bg != null)
                    slot.bg.rectTransform.localScale = Vector3.one;
    }

    void UpdateAim()
    {
        if (aimIdx < 0)
            return;
        HeroKingAbility hero = Hero;
        if (hero == null || !HeroKingAbility.IsCharging || MinionFieldController.Instance == null || !MinionFieldController.Instance.IsPickingPosition)
        {
            CancelAim();
            return;
        }
        if (slots != null && aimIdx < slots.Length && slots[aimIdx] != null && slots[aimIdx].bg != null)
            slots[aimIdx].bg.rectTransform.localScale = Vector3.one * (1.05f + 0.05f * Mathf.Sin(Time.unscaledTime * 10f));
        if (aimRing != null && Camera.main != null)
        {
            bool mouse = !Input.touchSupported || Input.mousePresent;
            aimRing.gameObject.SetActive(mouse);
            if (mouse)
            {
                Vector3 w = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                HeroSkillVisual.SetCircle(aimRing, w, hero.GetTargetRadius(aimIdx));
            }
        }
    }
    #endregion
}

// (2026-11-25) 스킬 칸 설명 말풍선 입력: 마우스 = 올리면 바로, 손가락 = holdTime 이상 누르고 있으면 (떼면 숨김, 그 클릭은 스킬 안 씀)
public class HeroSkillTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    HeroSkillHud hud;
    int idx;
    bool pressing;
    float pressTime;
    bool longPressed;
    bool hovering;

    public void Init(HeroSkillHud hud, int idx)
    {
        this.hud = hud;
        this.idx = idx;
    }

    static bool IsMouse(PointerEventData e) => e.pointerId < 0;

    public void OnPointerEnter(PointerEventData e)
    {
        if (!IsMouse(e) || hud == null)
            return;
        hovering = true;
        hud.ShowTooltip(idx);
    }

    public void OnPointerExit(PointerEventData e)
    {
        hovering = false;
        pressing = false;
        hud?.HideTooltip(idx);
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (IsMouse(e))
            return;
        pressing = true;
        longPressed = false;
        pressTime = Time.unscaledTime;
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (IsMouse(e))
            return;
        pressing = false;
        if (!hovering)
            hud?.HideTooltip(idx);
    }

    void Update()
    {
        if (pressing && !longPressed && hud != null && Time.unscaledTime - pressTime >= hud.tooltipHoldTime)
        {
            longPressed = true;
            hud.ShowTooltip(idx);
        }
    }

    void OnDisable()
    {
        pressing = hovering = false;
        hud?.HideTooltip(idx);
    }

    // 이번 클릭이 길게 누르기였으면 true (한 번만)
    public bool ConsumeLongPress()
    {
        bool v = longPressed;
        longPressed = false;
        return v;
    }
}
