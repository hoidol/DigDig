using System;
using UnityEngine;

public class KingSlimeManageCanvas : CanvasUI<KingSlimeManageCanvas>
{
    public MinionSlotPanel[] minionSlotPanels;
    // public OwnItemPanel[] ownItemPanels;


    void OnEnable()
    {
        Time.timeScale = 0;
        GameEventBus.Subscribe<AddedItemEvent>(OnAddedItemEvent);
    }

    void OnDisable()
    {
        Time.timeScale = 1;
        GameEventBus.Unsubscribe<AddedItemEvent>(OnAddedItemEvent);
    }
    public override void Init()
    {
        if (init)
            return;

        init = true;
        minionSlotPanels = GetComponentsInChildren<MinionSlotPanel>();
        // ownItemPanels = GetComponentsInChildren<OwnItemPanel>();
        for (int i = 0; i < minionSlotPanels.Length; i++)
        {
            minionSlotPanels[i].idx = i;
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
        // MinionFieldController.Instance.minionSlotCount
        // for (int i = 0; i < minionSlotPanels.Length; i++)
        // {
        //     if (i < MinionFieldController.Instance.minionSlotCount)
        //     {
        //         minionSlotPanels[i].gameObject.SetActive(true);
        //     }
        //     else
        //     {
        //         minionSlotPanels[i].gameObject.SetActive(false);
        //     }
        // }

        for (int i = 0; i < minionSlotPanels.Length; i++)
        {
            minionSlotPanels[i].UpdatePanel();
        }

        // for (int i = 0; i < ownItemPanels.Length; i++)
        // {
        //     ownItemPanels[i].SetItem(Character.Instance.itemInventory.GetItem(Character.Instance.itemInventory.ownItemKeys[i]), i);
        // }
    }
}