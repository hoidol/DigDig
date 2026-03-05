using UnityEngine;

public class UpgradeButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        Time.timeScale = 0;
        UpgradeCanvas.Instance.OpenCanvas(() =>
        {
            Time.timeScale = 1;

        });
    }
}
