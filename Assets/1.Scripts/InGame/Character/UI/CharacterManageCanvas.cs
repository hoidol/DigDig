using System;
using UnityEngine;

public class CharacterManageCanvas : CanvasUI<CharacterManageCanvas> 
{
    public SlimeSlotPanel[] slimeSlotPanels;
    public OwnItemPanel[] ownItemPanels;
    

    void OnEnable()
    {
        Time.timeScale= 0;
        GameEventBus.Subscribe<AddedItemEvent>(OnAddedItemEvent);
    }

    void OnDisable()
    {
        Time.timeScale= 1;
        GameEventBus.Unsubscribe<AddedItemEvent>(OnAddedItemEvent);
    }
    public override void Init()
    {
        if(init)
            return;

        init =true;
        slimeSlotPanels = GetComponentsInChildren<SlimeSlotPanel>();
        ownItemPanels = GetComponentsInChildren<OwnItemPanel>();
        for(int i = 0; i < slimeSlotPanels.Length; i++)
        {
            slimeSlotPanels[i].idx = i;
        }

    }

    void OnAddedItemEvent(AddedItemEvent e)
    {
        UpdateCanvas();
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
        // SlimeSpawner.Instance.slimeSlotCount
        for(int i = 0; i < slimeSlotPanels.Length; i++)
        {
            if(i < SlimeSpawner.Instance.slimeSlotCount)
            {
                slimeSlotPanels[i].gameObject.SetActive(true);
            }
            else
            {
                slimeSlotPanels[i].gameObject.SetActive(false);
            }
        }

        for(int i = 0; i < slimeSlotPanels.Length; i++)
        {
            slimeSlotPanels[i].UpdatePanel();
        }
    }
}