using System;
using UnityEngine;
using TMPro;
public class BlockCanvas : CanvasUI<BlockCanvas>
{
    public TMP_Text messageText;
    public void OpenCanvas(string message, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        messageText.text = message;
    }
}
