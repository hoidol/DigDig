using System;
using UnityEngine;
using TMPro;
public class SnakeCanvas : CanvasUI<SnakeCanvas> 
{
    SnakeSuggest snakeSuggest;
    public TMP_Text buffTitleText;
    public TMP_Text buffDescText;
    public TMP_Text nerfTitleText;
    public TMP_Text nerfDescText;
    public void OpenCanvas(SnakeSuggest sSuggest,Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);

        snakeSuggest = sSuggest;
        buffTitleText.text = snakeSuggest.BuffTitle();
        buffDescText.text = snakeSuggest.BuffDesc();
        nerfTitleText.text = snakeSuggest.NerfTitle();
        nerfDescText.text = snakeSuggest.NerfDesc();
    }

    public void OnClickAccept()
    {
        Player.Instance.AddBuff(snakeSuggest.buff);
        Player.Instance.AddBuff(snakeSuggest.nerf);
        CloseCanvas();
    }

    public void Reject()
    {
        CloseCanvas();
    }
}