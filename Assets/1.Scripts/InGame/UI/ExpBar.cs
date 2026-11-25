using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-20) 인게임 킹 경험치바 (화면 상단). KingSlime.AddExp → ExpChangedEvent
// slider를 씬에서 연결하면 그걸 쓰고, 비어 있으면 실행 중 이 오브젝트 크기에 맞춰 기본 Slider를 만든다
// (2026-11-21) 밀린 레벨업이 있으면 레벨 글씨 = "LEVEL UP!"(+밀린 수, 노란색). (2026-11-22) 경험치가 다 차면 레벨업 창이 바로 열림
public class ExpBar : MonoBehaviour
{
    public Slider slider;
    public TMP_Text levelText; // "Lv.N" (비우면 기본 모양에서 생성)
    public TMP_Text expText;   // "exp/max"
    public Image image;        // 이전 방식(Image fill). slider를 만들면 숨김
    public Color levelUpColor = new Color(1f, 0.85f, 0.2f);

    string lastLabel;
    Color normalLevelColor = Color.white;

    void Start()
    {
        if (slider == null)
            BuildDefaultSlider();
        if (levelText != null)
            normalLevelColor = levelText.color;
        GameEventBus.Subscribe<ExpChangedEvent>(OnExpChanged);

        KingSlime king = KingSlime.Instance;
        if (king != null)
            Refresh(king.exp, king.MaxExp, king.lv);
        else
            Refresh(0, KingSlime.GetMaxExp(0), 0);
    }

    void OnDestroy()
    {
        GameEventBus.Unsubscribe<ExpChangedEvent>(OnExpChanged);
    }

    void OnExpChanged(ExpChangedEvent e) => Refresh(e.exp, e.maxExp, e.lv);

    // 레벨 글씨 = 레벨업 상태 (밀린 수는 이벤트 없이도 바뀌므로 매 프레임 확인, 글씨가 바뀔 때만 갱신)
    void Update()
    {
        KingSlime king = KingSlime.Instance;
        if (king == null || levelText == null)
            return;
        int pending = king.pendingLevelUps;
        // (2026-11-23) 표시 레벨 = king.lv (Lv.0 시작, 최대 Lv.9)
        string label = pending <= 0 ? $"Lv.{king.lv}" : "LEVEL UP!" + (pending > 1 ? $" x{pending}" : "");
        if (label == lastLabel)
            return;
        lastLabel = label;
        levelText.text = label;
        levelText.color = pending > 0 ? levelUpColor : normalLevelColor;
    }

    void Refresh(int exp, int maxExp, int lv)
    {
        bool isMax = KingSlime.IsMaxLevel(lv);
        float ratio = isMax ? 1f : maxExp > 0 ? (float)exp / maxExp : 0f;
        if (slider != null)
            slider.value = ratio;
        if (image != null && image.gameObject.activeSelf)
            image.fillAmount = ratio;
        if (expText != null)
            expText.text = isMax ? "MAX" : $"{exp}/{maxExp}";
    }

    #region 기본 모양 (씬에 Slider 연결이 없을 때만)
    public void BuildDefaultSlider()
    {
        if (image != null)
            image.gameObject.SetActive(false);
        foreach (Image img in GetComponents<Image>())
            img.enabled = false;

        GameObject go = new GameObject("ExpSlider", typeof(RectTransform), typeof(Slider));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(transform, false);
        Stretch(rt, Vector2.zero, Vector2.zero);

        Image bg = NewImage("Background", rt, new Color(0f, 0f, 0f, 0.55f));
        Stretch(bg.rectTransform, Vector2.zero, Vector2.zero);

        RectTransform fillArea = new GameObject("Fill Area", typeof(RectTransform)).GetComponent<RectTransform>();
        fillArea.SetParent(rt, false);
        Stretch(fillArea, new Vector2(4, 4), new Vector2(-4, -4));
        Image fill = NewImage("Fill", fillArea, new Color(0.45f, 0.85f, 0.35f, 1f));
        Stretch(fill.rectTransform, Vector2.zero, Vector2.zero);

        slider = go.GetComponent<Slider>();
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
        slider.fillRect = fill.rectTransform;
        slider.handleRect = null;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 0;

        levelText = NewText("LevelText", rt, TextAlignmentOptions.MidlineLeft, new Vector2(16, 0), new Vector2(0, 0));
        expText = NewText("ExpText", rt, TextAlignmentOptions.MidlineRight, new Vector2(0, 0), new Vector2(-16, 0));
    }

    static void Stretch(RectTransform rt, Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    static Image NewImage(string name, Transform parent, Color color)
    {
        Image img = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        img.transform.SetParent(parent, false);
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    static TMP_Text NewText(string name, Transform parent, TextAlignmentOptions align, Vector2 offsetMin, Vector2 offsetMax)
    {
        TextMeshProUGUI text = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        RectTransform rt = text.rectTransform;
        rt.SetParent(parent, false);
        Stretch(rt, offsetMin, offsetMax);
        text.alignment = align;
        text.enableAutoSizing = true;
        text.fontSizeMin = 12;
        text.fontSizeMax = 32;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }
    #endregion
}

public class ExpChangedEvent
{
    public int exp;
    public int maxExp;
    public int lv;
    public ExpChangedEvent(int e, int mE, int lv = 0)
    {
        this.exp = e;
        this.maxExp = mE;
        this.lv = lv;
    }
}
