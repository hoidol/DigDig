using UnityEngine;

public class EquipmentButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        EquipmentCanvas.Instance.OpenCanvas();
    }

}
