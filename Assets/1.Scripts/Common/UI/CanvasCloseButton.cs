using UnityEngine;
using System.Collections;

public class CanvasCloseButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        //GetComponentInParent<CanvasUI<?>>
        //GetComponentInParent<Canvas>().gameObject.SetActive(false);
    }

}
