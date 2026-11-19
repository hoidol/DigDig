using System;
using UnityEngine;
using TMPro;
public class YesOrNoCanvas : CanvasUI<YesOrNoCanvas>
{
    public TMP_Text titleText;
    public TMP_Text bodyText;
    public Action<bool> resultCallback;
    public GameObject noButton; 
    public void OpenCanvas(string title,string body, Action<bool> rCallback, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        titleText.text = title;
        bodyText.text = body;
        resultCallback = rCallback;
        noButton.gameObject.SetActive(true); // onlyYes로 열었던 뒤에도 No 버튼이 보이게
    }
    public void OpenCanvas(string title, string body, bool onlyYes, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        titleText.text = title;
        bodyText.text = body;
        if (onlyYes)
        {
            noButton.gameObject.SetActive(false);
        }
    }

    public void OnClickedYes()
    {
        SoundManager.Instance.PlaySound(SFXType.Click);
        resultCallback?.Invoke(true);
        resultCallback = null;
        CloseCanvas();
    }
    public void OnClickedNo()
    {
        SoundManager.Instance.PlaySound(SFXType.Click);
        resultCallback?.Invoke(false);
        resultCallback = null;
        CloseCanvas();
    }
}
