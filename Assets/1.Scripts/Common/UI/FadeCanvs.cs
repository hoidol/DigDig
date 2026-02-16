using UnityEngine;
using System.Collections;
using System;
using DG.Tweening;
public class FadeCanvs : CanvasUI<FadeCanvs>
{
    CanvasGroup canvasGroup;
    public float fadeInSec = 0.7f; //어두워지는 시간  
    public float fadeOutSec = 0.5f; //밝아지는 시간  
    public void Awake()
    {
        canvasGroup = GetComponentInChildren<CanvasGroup>(true);
    }
    public void Fade(Action endFadeOut = null, Action endFadeIn = null)
    {
        gameObject.SetActive(true);
        StartCoroutine(CoFade(endFadeOut, endFadeIn));
    }
    IEnumerator CoFade(Action endFadeOut = null, Action endFadeIn = null)
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 0;
        yield return canvasGroup.DOFade(1, fadeInSec).WaitForCompletion();

        //가비지 컬렉터 동작시킴 - 모바일 테스트 필요
        GC.Collect();
        GC.WaitForPendingFinalizers();
        endFadeOut?.Invoke();
        yield return canvasGroup.DOFade(0, fadeOutSec).WaitForCompletion();
        endFadeIn?.Invoke();

        gameObject.SetActive(false);

    }
}
