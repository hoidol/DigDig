using UnityEngine;
using System.Collections;
using System;

public abstract class CanvasUI<T> : MonoSingleton<T> where T : MonoBehaviour
{
    public Action closeCallback;
    public virtual void OpenCanvas(Action closeCallback = null)
    {
        this.closeCallback = closeCallback;
        gameObject.SetActive(true);
    }
    public virtual void CloseCanvas()
    {
        //SoundMgr.Instance.PlaySound(SFXType.Click);
        gameObject.SetActive(false);
        closeCallback?.Invoke();
    }
}
