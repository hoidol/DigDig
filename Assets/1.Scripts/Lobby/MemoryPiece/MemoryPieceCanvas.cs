using System;
using UnityEngine;

public class MemoryPieceCanvas : CanvasUI<MemoryPieceCanvas>  
{
    public MemoryPieceAbilityGroupPanel[]     groupPanels;
    bool init;
    void Init()
    {
        if(init)
            return;
        init =true;
        groupPanels= GetComponentsInChildren<MemoryPieceAbilityGroupPanel>();
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