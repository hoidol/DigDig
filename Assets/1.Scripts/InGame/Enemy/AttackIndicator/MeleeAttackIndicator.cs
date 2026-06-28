using System;
using UnityEngine;
using DG.Tweening;
public class MeleeAttackIndicator : AreaIndicator
{
    public Transform warningTr;
    public override void PlayIndicator(float sec, Action end)
    {
        gameObject.SetActive(true);
        warningTr.localScale = Vector2.zero;
        warningTr.DOKill();
        warningTr.DOScale(1f, sec).SetEase(Ease.InCubic).OnComplete(() =>
        {
            end.Invoke();
            gameObject.SetActive(false);
        });
    }
    public override void StopIndicator()
    {
        warningTr.DOKill();
        gameObject.SetActive(false);
    }
}
