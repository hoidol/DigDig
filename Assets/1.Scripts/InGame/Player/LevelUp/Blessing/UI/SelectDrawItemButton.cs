using UnityEngine;

public class SelectDrawItemButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        BlessingCanvas.Instance.CloseCanvas();
        SelectItemCanvas.Instance.OpenCanvas(() =>
        {
            Time.timeScale = 1;
        });

    }
}