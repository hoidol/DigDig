using System;
using UnityEngine;
using TMPro;
public class BlockCanvas : CanvasUI<BlockCanvas>
{
    public TMP_Text messageText;
    public override void OpenCanvas(Action closeCallback = null)
    {
        messageText.text ="";
        base.OpenCanvas(closeCallback);
    }
    public void OpenCanvas(string message, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        messageText.text = message;
    }
}
