using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LevelUpSlimeCanvas : CanvasUI<LevelUpSlimeCanvas>
{
    public LevelUpSlimeSlotPanel[] slimeSlotPanels;

    public void OpenCanvas(Slime[] slimes, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        for(int i = 0; i < slimeSlotPanels.Length; i++)
        {
            if(i < slimes.Length)
            {
                slimeSlotPanels[i].gameObject.SetActive(true);
                slimeSlotPanels[i].SetSlime(slimes[i]);
            }               
            else
                slimeSlotPanels[i].gameObject.SetActive(false);
        }
        Init();   
        Open();
    }

    public void Open()
    {
        UpdateCanvas();
    }

    public void UpdateCanvas()
    {

    }
}
