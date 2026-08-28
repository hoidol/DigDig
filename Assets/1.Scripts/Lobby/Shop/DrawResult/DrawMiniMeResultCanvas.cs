using System;
using UnityEngine;

namespace Lobby
{
    public class DrawMiniMeResultCanvas : DrawResultCanvas<DrawMiniMeResultCanvas> 
{
    public MiniMePanel[] miniMePanels;
    public override void OpenCanvas(string[] pickedKeys, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        for(int i = 0; i < miniMePanels.Length; i++)
        {
            if(i < pickedKeys.Length)
            {
                miniMePanels[i].gameObject.SetActive(true);
                miniMePanels[i].SetData(MiniMeManager.Instance.GetMiniMeData(pickedKeys[i]));
            }
            else
            {
                miniMePanels[i].gameObject.SetActive(false);
            }
            
        }
        
    }
}
}
