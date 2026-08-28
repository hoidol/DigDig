using System;
using UnityEngine;

public class CharacterManageCanvas : CanvasUI<CharacterManageCanvas> 
{
    public MiniMeSlotPanel[] miniMeSlotPanels;
    public OwnItemPanel[] ownItemPanels;
    

    void OnEnable()
    {
        Time.timeScale= 0;
    }

    void OnDisable()
    {
        Time.timeScale= 1;
    }
    public override void Init()
    {
        if(init)
            return;

        init =true;
        miniMeSlotPanels = GetComponentsInChildren<MiniMeSlotPanel>();
        ownItemPanels = GetComponentsInChildren<OwnItemPanel>();
        for(int i = 0; i < miniMeSlotPanels.Length; i++)
        {
            miniMeSlotPanels[i].idx = i;
        }

    }
    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        Init();   
        Open();
    }

    public void Open()
    {
        UpdateCanvas();
    }

    public void UpdateCanvas()
    {
        // MiniMeSpawner.Instance.miniMeSlotCount
        for(int i = 0; i < miniMeSlotPanels.Length; i++)
        {
            if(i < MiniMeSpawner.Instance.miniMeSlotCount)
            {
                miniMeSlotPanels[i].gameObject.SetActive(true);
            }
            else
            {
                miniMeSlotPanels[i].gameObject.SetActive(false);
            }
        }

        for(int i = 0; i < miniMeSlotPanels.Length; i++)
        {
            miniMeSlotPanels[i].UpdatePanel();
        }
    }
}