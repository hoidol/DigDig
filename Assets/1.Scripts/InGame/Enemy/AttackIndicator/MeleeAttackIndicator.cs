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
        warningTr.DOScale(1f, 0.4f).SetEase(Ease.InCubic).OnComplete(() =>
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
