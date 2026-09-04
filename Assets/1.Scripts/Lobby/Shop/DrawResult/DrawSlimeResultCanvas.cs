using System;
using UnityEngine;

namespace Lobby
{
    public class DrawSlimeResultCanvas : DrawResultCanvas<DrawSlimeResultCanvas> 
{
    public SlimePanel[] slimePanels;
    public override void OpenCanvas(string[] pickedKeys, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        for(int i = 0; i < slimePanels.Length; i++)
        {
            if(i < pickedKeys.Length)
            {
                slimePanels[i].gameObject.SetActive(true);
                slimePanels[i].SetData(SlimeManager.Instance.GetSlimeData(pickedKeys[i]));
            }
            else
            {
                slimePanels[i].gameObject.SetActive(false);
            }
            
        }
        
    }
}
}
