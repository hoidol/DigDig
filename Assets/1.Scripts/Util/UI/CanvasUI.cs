using UnityEngine;
using System.Collections;
using System;

public abstract class CanvasUI<T> : MonoSingleton<T> where T : MonoBehaviour
{
    public Action closeCallback;
    [HideInInspector] public bool init;
    public virtual void Init()
    {
        if(init)
            return ;
        init= true;

        return ;
    }
    // 창이 열릴 때 소리. 팝업이 아닌 창(페이드·블록 등)은 빈 재정의, 자기 소리가 있는 창은 다른 소리로 재정의
    protected virtual void PlayOpenSound()
    {
        SoundManager.Play(SFXType.PopupOpen);
    }
    public virtual void OpenCanvas(Action closeCallback = null)
    {
        this.closeCallback = closeCallback;
        // 이미 열려 있는 창을 다시 여는 경우는 소리 없음
        if (!gameObject.activeSelf)
            PlayOpenSound();
        gameObject.SetActive(true);
    }
    public virtual void CloseCanvas()
    {
        //SoundMgr.Instance.PlaySound(SFXType.Click);
        gameObject.SetActive(false);
        closeCallback?.Invoke();
    }
}
