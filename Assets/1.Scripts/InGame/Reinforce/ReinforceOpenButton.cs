using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ReinforceOpenButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        ReinforceCanvas.Instance.OpenCanvas();
    }
}
