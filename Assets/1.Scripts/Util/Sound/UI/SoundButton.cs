using UnityEngine;
using System.Collections;

public class SoundButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        SoundMgr.Instance.PlaySound(SFXType.Click);
    }


}
