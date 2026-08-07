using System;
using System.Collections.Generic;
using UnityEngine;

public class PopupCanvas : CanvasUI<PopupCanvas>
{
    public PopupPanel popupPanelPrefab;
    public List<PopupPanel> popupPanels = new List<PopupPanel>();
    public RectTransform startTr;
    public RectTransform parentTr;
    //public List<string> messages = new List<string>(); 
    public void OpenCanvas(string message, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        //Debug.Log($"message {message}");
        PopupPanel popup =  GetPopupPanel();
        popup.transform.position = startTr.position;
        popup.Show(message);
        
    }

    PopupPanel GetPopupPanel()
    {
        for(int i =0;i< popupPanels.Count; i++)
        {
            if (popupPanels[i].gameObject.activeSelf)
                continue;

            popupPanels[i].gameObject.SetActive(true);
            return popupPanels[i];
        }


        PopupPanel panel = Instantiate(popupPanelPrefab, parentTr);
        return panel;
    }

}
