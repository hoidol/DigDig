using UnityEngine;
using UnityEngine.UI;

public class SlimeMergeDragUI : MonoSingleton<SlimeMergeDragUI>
{
    public Image image;
    RectTransform rectTransform;
    Canvas canvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        image.raycastTarget = false; // 드래그 중인 아이콘이 드롭 대상 슬롯의 레이캐스트를 가리면 안 됨
    }

    public void SetSlime(Slime slime)
    {
        SlimeData slimeData = SlimeManager.Instance.GetSlimeData(slime.key);
        image.sprite = slimeData.thum;
        gameObject.SetActive(true);
    }

    public void SetPosition(Vector2 screenPosition)
    {
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, screenPosition, cam, out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}