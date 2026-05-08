using UnityEngine;
using System.Collections;
using System;
using DG.Tweening;
using TMPro;
public class FadeCanvs : CanvasUI<FadeCanvs>
{
    [SerializeField] CanvasGroup canvasGroup;
    public float fadeInSec = 0.7f; //어두워지는 시간  
    public float fadeOutSec = 0.5f; //밝아지는 시간  
    public TMP_Text text;
    public void FadeIn(string msg = null, Action endFadeIn = null)
    {
        canvasGroup.alpha = 1;
        gameObject.SetActive(true);
        text.text = msg;
        StartCoroutine(CoFadeIn(endFadeIn));
    }

    public void FadeOutIn(string msg = null, Action endFadeOut = null, Action endFadeIn = null)
    {
        canvasGroup.alpha = 0;
        gameObject.SetActive(true);
        text.text = msg;
        StartCoroutine(CoFadeOutIn(endFadeOut, endFadeIn));
    }

    IEnumerator CoFadeOutIn(Action endFadeOut = null, Action endFadeIn = null)
    {
        yield return canvasGroup.DOFade(1, fadeInSec).WaitForCompletion();

#if !UNITY_EDITOR
        if (!string.IsNullOrEmpty(text.text))
            yield return new WaitForSeconds(2);
#endif

        //가비지 컬렉터 동작시킴 - 모바일 테스트 필요
        GC.Collect();
        GC.WaitForPendingFinalizers();
        endFadeOut?.Invoke();
        yield return CoFadeIn(endFadeIn);
        gameObject.SetActive(false);
    }

    IEnumerator CoFadeIn(Action endFadeIn = null)
    {
#if !UNITY_EDITOR
        if (!string.IsNullOrEmpty(text.text))
            yield return new WaitForSeconds(2);
#endif
        yield return canvasGroup.DOFade(0, fadeOutSec).WaitForCompletion();
        endFadeIn?.Invoke();
        gameObject.SetActive(false);
    }
}
