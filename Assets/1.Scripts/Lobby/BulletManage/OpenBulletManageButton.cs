using UnityEngine;

public class OpenBulletManageButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        BulletManageCanvas.Instance.OpenCanvas();
    }
}