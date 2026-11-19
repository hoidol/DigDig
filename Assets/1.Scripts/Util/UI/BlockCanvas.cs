using System;
using UnityEngine;
using TMPro;
public class BlockCanvas : CanvasUI<BlockCanvas>
{
    public TMP_Text messageText;
    // 입력 막기용 창이라 열리는 소리 없음
    protected override void PlayOpenSound() { }
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
