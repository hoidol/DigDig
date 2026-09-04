using System;
using UnityEngine;

public class BaseLobbyCanvas : MonoBehaviour
{
    public LobbyState state;

    [HideInInspector] public bool init;
    public virtual void Init()
    {
        if(init)
            return ;
        init= true;

        return ;
    }

    Action closeCallback;
    public virtual void OpenCanvas(Action closeCallback = null)
    {
        gameObject.SetActive(true);
        this.closeCallback = closeCallback;
    }
    public virtual void UpdateCanvas()
    {
        
    }

    public virtual void CloseCanvas()
    {
        closeCallback?.Invoke();
        gameObject.SetActive(false);
    }

}