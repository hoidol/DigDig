using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ToastPanel : MonoBehaviour
{
    public TMP_Text messageText;
    public RectTransform rectTransform;
    public void Toast(string message, Action<ToastPanel> onComplete)
    {
        messageText.text = message;
        if(rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        rectTransform.DOKill();
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.DOAnchorPos(rectTransform.anchoredPosition + new Vector2(0, 150), 3f).OnComplete(() =>
        {
            onComplete?.Invoke(this);
        });
    }
}
