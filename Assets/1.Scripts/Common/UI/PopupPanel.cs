using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;
public class PopupPanel : MonoBehaviour
{
    public TMP_Text text;
    RectTransform rectTransform;
    public void Show(string message)
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        rectTransform.DOAnchorPos(rectTransform.anchoredPosition + new Vector2(0, 200f),3).OnComplete(()=> {
            gameObject.SetActive(false);
        });
        text.text = message;
        

    }
}
