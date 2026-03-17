using System;
using UnityEngine;
using DG.Tweening;
public class MeleeAttackIndicator : AttackIndicator
{
    public Transform warningTr;
    public override void PlayIndicator(Action end)
    {
        gameObject.SetActive(true);
        warningTr.localScale = Vector2.zero;
        warningTr.DOScale(1, 1).SetEase(Ease.InCubic).OnComplete(() =>
        {
            end.Invoke();
            gameObject.SetActive(false);
        });
    }

}
