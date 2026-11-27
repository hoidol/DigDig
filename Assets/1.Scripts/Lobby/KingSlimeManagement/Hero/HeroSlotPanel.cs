using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-26) [용사] 탭 용사 칸 하나: 그림 + 이름, 미보유 = 어둡게 + 자물쇠 글씨 "Lv.N", 선택 = 금색 테두리
// 씬에서 만들어 HeroCollectionContainer.slotTemplate에 연결하면 그걸 복사, 없으면 Create로 기본 모양
public class HeroSlotPanel : MonoBehaviour
{
    public Button button;
    public Image bgImage;
    public Image thumImage;
    public TMP_Text nameText;
    public GameObject lockObject;
    public TMP_Text lockText;
    public GameObject selectedObject; // 선택 표시 (비우면 Outline으로)

    static readonly Color LOCKED_THUM = new Color(0.15f, 0.15f, 0.18f, 1f);
    static readonly Color SELECTED_OUTLINE = new Color(1f, 0.82f, 0.2f, 1f);

    HeroCollectionContainer container;
    HeroDataTable.HeroData hero;
    Outline outline;

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(() => container?.OnClickedSlot(hero));
    }

    public void Set(HeroCollectionContainer owner, HeroDataTable.HeroData data, bool owned, bool selected)
    {
        container = owner;
        hero = data;
        AbilityData card = HeroCollectionContainer.GetHeroCard(data.key);
        if (thumImage != null)
        {
            thumImage.sprite = card != null ? card.thum : null;
            thumImage.enabled = thumImage.sprite != null;
            thumImage.color = owned ? Color.white : LOCKED_THUM;
        }
        if (nameText != null)
            nameText.text = card != null && !string.IsNullOrEmpty(card.title) ? card.title : data.key;
        if (lockObject != null)
            lockObject.SetActive(!owned);
        if (lockText != null)
            lockText.text = !data.isFree ? TranslateManager.GetText("HeroPass_Label") : $"Lv.{data.unlockLv}"; // (2026-11-27) 패스 용사 = "용사 패스"
        if (selectedObject != null)
            selectedObject.SetActive(selected);
        else
        {
            if (outline == null && bgImage != null)
            {
                outline = bgImage.gameObject.AddComponent<Outline>();
                outline.effectDistance = new Vector2(6f, -6f);
                outline.effectColor = SELECTED_OUTLINE;
            }
            if (outline != null)
                outline.enabled = selected;
        }
    }

    static bool UserLevelOk(HeroDataTable.HeroData d) => HeroCollectionContainer.UserLevel >= d.unlockLv;

    // 기본 모양: 칸 바탕(styleSource = 수련 탭 칸의 바탕 그림을 빌림) + 그림 + 이름 + 자물쇠 글씨
    public static HeroSlotPanel Create(RectTransform parent, Image styleSource)
    {
        var go = new GameObject("HeroSlot", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var slot = go.AddComponent<HeroSlotPanel>();
        slot.bgImage = go.GetComponent<Image>();
        if (styleSource != null && styleSource.sprite != null)
        {
            slot.bgImage.sprite = styleSource.sprite;
            slot.bgImage.type = styleSource.type;
            slot.bgImage.color = styleSource.color;
        }
        else
            slot.bgImage.color = new Color(0.2f, 0.18f, 0.3f, 1f);
        slot.button = go.GetComponent<Button>();
        slot.button.targetGraphic = slot.bgImage;

        RectTransform thum = NewRect("Thum", (RectTransform)go.transform, new Vector2(0.12f, 0.28f), new Vector2(0.88f, 0.94f));
        slot.thumImage = thum.gameObject.AddComponent<Image>();
        slot.thumImage.preserveAspect = true;
        slot.thumImage.raycastTarget = false;

        RectTransform name = NewRect("Name", (RectTransform)go.transform, new Vector2(0.04f, 0.03f), new Vector2(0.96f, 0.26f));
        slot.nameText = NewText(name, 34f, Color.white);

        RectTransform lockRt = NewRect("Lock", (RectTransform)go.transform, new Vector2(0f, 0.28f), new Vector2(1f, 0.94f));
        slot.lockObject = lockRt.gameObject;
        slot.lockText = NewText(lockRt, 44f, new Color(1f, 0.85f, 0.4f));
        slot.lockText.fontStyle = FontStyles.Bold;
        return slot;
    }

    static RectTransform NewRect(string name, RectTransform parent, Vector2 min, Vector2 max)
    {
        var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    static TMP_Text NewText(RectTransform rt, float size, Color color)
    {
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.fontSize = size;
        t.enableAutoSizing = true;
        t.fontSizeMin = 16f;
        t.fontSizeMax = size;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }
}
