using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-20) 인게임 킹 경험치바 (화면 상단). KingSlime.AddExp → ExpChangedEvent
// slider를 씬에서 연결하면 그걸 쓰고, 비어 있으면 실행 중 이 오브젝트 크기에 맞춰 기본 Slider를 만든다
public class ExpBar : MonoBehaviour
{
    public Slider slider;
    public TMP_Text levelText; // "Lv.N" (비우면 기본 모양에서 생성)
    public TMP_Text expText;   // "exp/max"
    public Image image;        // 이전 방식(Image fill). slider를 만들면 숨김

    void Start()
    {
        if (slider == null)
            BuildDefaultSlider();
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

    void Refresh(int exp, int maxExp, int lv)
    {
        float ratio = maxExp > 0 ? (float)exp / maxExp : 0f;
        if (slider != null)
            slider.value = ratio;
        if (image != null && image.gameObject.activeSelf)
            image.fillAmount = ratio;
        if (levelText != null)
            levelText.text = $"Lv.{lv + 1}";
        if (expText != null)
            expText.text = $"{exp}/{maxExp}";
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
