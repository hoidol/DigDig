using UnityEngine;
using DG.Tweening;
using System;

// 낙하 경고 표시 오브젝트
// 인스펙터에서 경고 스프라이트/이펙트 오브젝트를 warningVisual에 연결
public class DropWarning : MonoBehaviour
{
    [SerializeField] Transform warningVisual;
    [SerializeField] GameObject strikeEffect; // 충격 이펙트 (선택)

    public void Play(float duration, float hitRadius, float damage, Action onStrike = null)
    {
        warningVisual.localScale = Vector3.zero;
        warningVisual.DOScale(1f, duration).SetEase(Ease.OutQuad)
            .OnComplete(() => Strike(hitRadius, damage, onStrike));
    }

    void Strike(float hitRadius, float damage, Action onStrike)
    {
        if (strikeEffect != null)
            strikeEffect.SetActive(true);

        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, hitRadius);
        foreach (var col in cols)
        {
            if (col.CompareTag("Player"))
            {
                Player.Instance.TakeDamage(damage);
                break;
            }
        }

        onStrike?.Invoke();
        Destroy(gameObject, 0.3f);
    }

    public void Cancel()
    {
        warningVisual.DOKill();
        Destroy(gameObject);
    }
}
