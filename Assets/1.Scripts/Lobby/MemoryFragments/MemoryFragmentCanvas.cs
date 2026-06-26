using System;
using UnityEngine;

public class MemoryFragmentCanvas : CanvasUI<MemoryFragmentCanvas>  
{
    public MemoryFragmentAbilityGroupPanel[]     groupPanels;
    bool init;
    void Init()
    {
        if(init)
            return;
        init =true;
        groupPanels= GetComponentsInChildren<MemoryFragmentAbilityGroupPanel>();
        for(int i = 0; i < groupPanels.Length; i++)
        {
            
        }
    }

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        Init();


    }
}