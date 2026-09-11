using System;
using UnityEngine;

namespace Lobby
{
    public class DrawSlimeResultCanvas : DrawResultCanvas<DrawSlimeResultCanvas> 
{
    public DrawSlimePanel[] drawSlimePanels;
    public override void OpenCanvas(string[] pickedKeys, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        for(int i = 0; i < drawSlimePanels.Length; i++)
        {
            if(i < pickedKeys.Length)
            {
                drawSlimePanels[i].gameObject.SetActive(true);
                drawSlimePanels[i].SetData(SlimeManager.Instance.GetSlimeData(pickedKeys[i]));
            }
            else
            {
                drawSlimePanels[i].gameObject.SetActive(false);
            }
            
        }
        
    }
}
}
