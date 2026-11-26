using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// wave 카드 한 장. 드래그하면 그림이 손가락을 따라가고, 놓은 위치의 타일에 소환/합성 (MinionOfferSystem.TryPlace)
// (2026-11-21) 드래그하지 않고 누르면 카드 줄 위에 미니언 설명 (MinionOfferCardInfo), 드래그를 시작하면 닫힘
public class MinionOfferCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Image thumImage;
    public Canvas rootCanvas;
    public TMPro.TMP_Text priceText; // 주사위 코인 가격 (선택, 비우면 표시 안 함)
    public GameObject mergeLvBadge;   // (2026-11-22) 머지 레벨 배지 (미니언 머리 위 MergeLv와 같은 모양, 드래그 그림에도 복사)
    public TMPro.TMP_Text mergeLvText; // 머지 레벨 = mergeLevel + 1 (MinionGameStatePanel.GetMergeLevelText)

    static readonly Color UNAFFORDABLE_COLOR = new Color(0.45f, 0.45f, 0.45f, 1f);

    MinionOfferSystem system;
    int handIdx;
    MinionData data;
    bool affordable = true;
    RectTransform ghost; // 드래그 중 따라다니는 그림

    static readonly Color DiscountPriceColor = new Color(0.45f, 1f, 0.45f); // (2026-11-25) 용사 강화로 가격이 깎인 카드

    // (2026-11-26) 머지 레벨이 높은 카드(표시 Lv.2 이상) 강조: 카드 바탕 금색 + 금색 테두리 반짝임 + 배지 금색·통통, 지급될 때 카드가 튀어오름
    static readonly Color HIGH_LEVEL_BG_COLOR = new Color(1f, 0.86f, 0.35f, 1f);
    static readonly Color HIGH_LEVEL_GLOW_COLOR = new Color(1f, 0.8f, 0.1f, 1f);
    static readonly Color HIGH_LEVEL_BADGE_COLOR = new Color(1f, 0.75f, 0.1f, 1f);
    const float GLOW_DISTANCE = 7f;

    bool highLevel;
    Image bgImage;
    Color bgBaseColor;
    Outline glow;
    Image badgeImage;
    Color badgeBaseColor;
    bool styleInit;

    void InitStyle()
    {
        if (styleInit)
            return;
        styleInit = true;
        bgImage = GetComponent<Image>();
        if (bgImage != null)
        {
            bgBaseColor = bgImage.color;
            glow = bgImage.gameObject.AddComponent<Outline>();
            glow.effectDistance = new Vector2(GLOW_DISTANCE, -GLOW_DISTANCE);
            glow.useGraphicAlpha = false;
            glow.enabled = false;
        }
        if (mergeLvBadge != null)
        {
            badgeImage = mergeLvBadge.GetComponent<Image>();
            if (badgeImage != null)
                badgeBaseColor = badgeImage.color;
        }
    }

    void ApplyLevelStyle(int mergeLevel)
    {
        InitStyle();
        highLevel = mergeLevel >= 1;
        if (bgImage != null)
            bgImage.color = highLevel ? HIGH_LEVEL_BG_COLOR : bgBaseColor;
        if (glow != null)
            glow.enabled = highLevel;
        if (badgeImage != null)
            badgeImage.color = highLevel ? HIGH_LEVEL_BADGE_COLOR : badgeBaseColor;
        if (!highLevel && mergeLvBadge != null)
            mergeLvBadge.transform.localScale = Vector3.one;
    }

    void Update()
    {
        if (!highLevel)
            return;
        float wave = Mathf.Sin(Time.unscaledTime * 5f);
        if (glow != null)
        {
            Color c = HIGH_LEVEL_GLOW_COLOR;
            c.a = 0.6f + 0.4f * wave;
            glow.effectColor = c;
        }
        if (mergeLvBadge != null)
            mergeLvBadge.transform.localScale = Vector3.one * (1.1f + 0.1f * wave);
    }

    // 새 카드로 지급될 때 (MinionOfferSystem.DealOffers): 높은 레벨 카드만 튀어오름
    public void PlayDealPop()
    {
        if (!highLevel || !gameObject.activeInHierarchy)
            return;
        transform.DOKill(true);
        transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * 0.25f, 0.45f, 6, 0.6f).SetUpdate(true);
    }

    void OnDisable()
    {
        transform.DOKill(true);
        transform.localScale = Vector3.one;
    }

    public void SetData(MinionOfferSystem system, int handIdx, MinionData data, int mergeLevel, int price, bool affordable, bool discounted = false)
    {
        if (mergeLvText != null)
            mergeLvText.text = MinionGameStatePanel.GetMergeLevelText(mergeLevel);
        ApplyLevelStyle(mergeLevel);
        this.system = system;
        this.handIdx = handIdx;
        this.affordable = affordable;
        this.data = data;
        thumImage.sprite = data != null ? data.thum : null;
        if (ghost == null)
            thumImage.color = NormalColor;
        if (priceText != null)
        {
            priceText.text = price.ToString();
            priceText.color = !affordable ? Color.red : discounted ? DiscountPriceColor : Color.white;
        }
    }

    // 주사위 코인이 모자라면 카드 그림을 어둡게
    Color NormalColor => affordable ? Color.white : UNAFFORDABLE_COLOR;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging || system == null)
            return;
        system.ToggleCardInfo(handIdx, data, (RectTransform)transform);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (system != null)
            system.HideCardInfo();
        GameObject go = new GameObject("DragGhost", typeof(RectTransform), typeof(Image));
        ghost = go.GetComponent<RectTransform>();
        ghost.SetParent(rootCanvas.transform, false);
        ghost.sizeDelta = thumImage.rectTransform.rect.size;
        Image img = go.GetComponent<Image>();
        img.sprite = thumImage.sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
        if (highLevel) // 드래그 그림에도 금색 테두리
        {
            Outline o = go.AddComponent<Outline>();
            o.effectColor = HIGH_LEVEL_GLOW_COLOR;
            o.effectDistance = new Vector2(4f, -4f);
        }
        thumImage.color = new Color(1f, 1f, 1f, 0.3f);
        // 드래그 그림에도 머지 레벨 배지 (그림 왼쪽 위)
        if (mergeLvBadge != null)
        {
            RectTransform badge = (RectTransform)Instantiate(mergeLvBadge, ghost).transform;
            badge.anchorMin = badge.anchorMax = new Vector2(0f, 1f);
            badge.anchoredPosition = Vector2.zero;
            foreach (Graphic g in badge.GetComponentsInChildren<Graphic>())
                g.raycastTarget = false;
        }
        if (system != null)
            system.IsCardDragging = true;
        MoveGhost(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveGhost(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ghost != null)
            Destroy(ghost.gameObject);
        ghost = null;
        thumImage.color = NormalColor;
        if (system != null)
            system.IsCardDragging = false;

        Vector3 world = Camera.main.ScreenToWorldPoint(eventData.position);
        if (system != null)
            system.TryPlace(handIdx, world); // 실패하면 카드는 그대로 남음
    }

    void MoveGhost(PointerEventData eventData)
    {
        if (ghost == null)
            return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)rootCanvas.transform, eventData.position, null, out Vector2 local);
        ghost.anchoredPosition = local;
    }
}
