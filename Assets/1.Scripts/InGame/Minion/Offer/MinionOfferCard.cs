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

    public void SetData(MinionOfferSystem system, int handIdx, MinionData data, int mergeLevel, int price, bool affordable)
    {
        if (mergeLvText != null)
            mergeLvText.text = MinionGameStatePanel.GetMergeLevelText(mergeLevel);
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
            priceText.color = affordable ? Color.white : Color.red;
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
