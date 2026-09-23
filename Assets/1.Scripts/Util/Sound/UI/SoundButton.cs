using UnityEngine;
using System.Collections;

public class SoundButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        SoundManager.Instance.PlaySound(SFXType.Click);
    }


}
