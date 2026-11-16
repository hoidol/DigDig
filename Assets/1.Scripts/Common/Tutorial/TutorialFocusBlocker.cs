using UnityEngine;

// 튜토리얼 포커스의 터치 막기 (전체 화면 투명 Image에 붙임). 구멍 안 터치는 통과(아래 UI가 받음), 밖은 막는다
// 어두운 화면(TutorialFocusDim)은 구멍 Mask의 자식이라 Mask의 레이캐스트 필터 때문에 구멍 밖을 못 막음 → 따로 둔다
public class TutorialFocusBlocker : MonoBehaviour, ICanvasRaycastFilter
{
    public RectTransform hole;

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (hole == null || !hole.gameObject.activeInHierarchy)
            return true;
        return !RectTransformUtility.RectangleContainsScreenPoint(hole, screenPoint, eventCamera);
    }
}
