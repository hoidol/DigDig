using UnityEngine;
using System.Collections;

public class ClickSoundButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        SoundMgr.Instance.PlaySound(SFXType.Click);
    }

    
}
