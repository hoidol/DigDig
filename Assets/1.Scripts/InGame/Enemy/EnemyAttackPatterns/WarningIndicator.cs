using UnityEngine;
using DG.Tweening;
using System;

// 낙하 경고 표시 오브젝트
// 인스펙터에서 경고 스프라이트/이펙트 오브젝트를 warningVisual에 연결
public class WarningIndicator : MonoBehaviour
{
    public static WarningIndicatorPoolingSystem poolingSystem = new();
    [SerializeField] Transform warningVisual;
    [SerializeField] Transform areaTr;

    public static WarningIndicator Instantiate(Vector2 pos, float size)
    {
        WarningIndicator warningIndicator = poolingSystem.Get(pos);
        warningIndicator.areaTr.localScale = new Vector3(size, size, 1);
        return warningIndicator;
    }


    public void Play(float duration, Action<WarningIndicator> end = null)
    {
        warningVisual.localScale = Vector3.zero;
        warningVisual.DOScale(1f, duration).SetEase(Ease.Linear)
            .OnComplete(() => end?.Invoke(this));
    }

    public void Cancel()
    {
        warningVisual.DOKill();
        gameObject.SetActive(false);
    }

}
