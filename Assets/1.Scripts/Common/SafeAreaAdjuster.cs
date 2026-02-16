using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaAdjuster : MonoBehaviour
{
    private RectTransform rectTransform;

    public CanvasScaler canvasScaler; // Canvas Scaler 컴포넌트 참조

    private Rect lastSafeArea;
    private Rect lastSafeAreaExcepBN;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasScaler = GetComponentInParent<CanvasScaler>();
        ApplySafeArea();
    }

    void Update()
    {
        // Safe Area가 변경되는 경우(예: 화면 회전)에 다시 적용
        if (Screen.safeArea != lastSafeAreaExcepBN)
        {
            ApplySafeArea();
        }


    }

    private void ApplySafeArea()
    {
        // 현재 기기의 Safe Area 가져오기
        Rect safeArea = Screen.safeArea;

        float scaleFactor = GetCanvasScaleFactor(); // Canvas의 스케일 팩터 계산

        // 배너 높이를 Reference Resolution 기준으로 변환하여 적용
        float adjustedBannerHeight = 0 / scaleFactor;
        // 배너 높이만큼 Safe Area 조정 (하단 배너 기준)
        safeArea.height -= adjustedBannerHeight;

        // Safe Area를 RectTransform의 비율로 변환
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // RectTransform에 앵커 값 적용
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        // Safe Area 업데이트
        lastSafeArea = safeArea;
        lastSafeAreaExcepBN = lastSafeArea;
        lastSafeAreaExcepBN.height = lastSafeArea.height;
    }

    float GetCanvasScaleFactor()
    {
        if (canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            Vector2 referenceResolution = canvasScaler.referenceResolution;
            float scaleFactor = Mathf.Min(Screen.width / referenceResolution.x, Screen.height / referenceResolution.y);
            return scaleFactor;
        }
        return 1f; // 기본값
    }

}
