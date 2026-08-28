using TMPro;
using UnityEngine;

public class CharacterManageButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        CharacterManageCanvas.Instance.OpenCanvas();
    }
}
