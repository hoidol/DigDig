using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ResetScrollRect : MonoBehaviour
{
    ScrollRect scrollRect;
    private void OnEnable()
    {
        if (scrollRect == null)
            scrollRect = GetComponent<ScrollRect>();


        scrollRect.verticalNormalizedPosition = 1;
    }
}
