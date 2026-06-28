using UnityEngine;
using DG.Tweening;
using System;

// 낙하 경고 표시 오브젝트
// 인스펙터에서 경고 스프라이트/이펙트 오브젝트를 warningVisual에 연결
public class SweepWarningIndicator : MonoBehaviour
{
    [SerializeField] Transform warningVisual;

    public void Play(float duration, Action end = null)
    {
        warningVisual.localScale = Vector3.zero;
        warningVisual.DOScale(1f, duration).SetEase(Ease.OutQuad)
            .OnComplete(() => end?.Invoke());
    }

    public void Cancel()
    {
        warningVisual.DOKill();
    }

}
