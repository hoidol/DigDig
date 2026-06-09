using UnityEngine;

public class SelectReinforceButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        BlessingCanvas.Instance.CloseCanvas();
    }
}