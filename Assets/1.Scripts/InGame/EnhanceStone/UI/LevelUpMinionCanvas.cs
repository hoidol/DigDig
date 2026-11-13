using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LevelUpMinionCanvas : CanvasUI<LevelUpMinionCanvas>
{
    public LevelUpMinionSlotPanel[] minionSlotPanels;

    public void OpenCanvas(Minion[] minions, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        for(int i = 0; i < minionSlotPanels.Length; i++)
        {
            if(i < minions.Length)
            {
                minionSlotPanels[i].gameObject.SetActive(true);
                minionSlotPanels[i].SetMinion(minions[i]);
            }               
            else
                minionSlotPanels[i].gameObject.SetActive(false);
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
