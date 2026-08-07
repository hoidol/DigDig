using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

// 크게 휘두르기 패턴
// 플레이어 방향으로 부채꼴 범위 공격
// 페이즈가 높을수록 범위와 피해 증가
// 가까워졌을때 발동되야 재밌는데.. 
public class SweepPattern : EnemyAttackPattern
{
    // [SerializeField] WarningIndicator warningIndicator;
    WarningIndicator warningIndicator;
    [SerializeField] float damageMultiplier = 1.5f;
    [SerializeField] float sweepSize = 5;
    DamageData damageData = new DamageData();
    public async override UniTask Execute(IEnemySpecialAttackPattern enemy, Action onEnd)
    {
        await base.Execute(enemy, onEnd);

        Debug.Log("SweepPattern Execute 1");
        Vector2 dir = (Character.Instance.transform.position - transform.position).normalized;
        warningIndicator = WarningIndicator.Instantiate((Vector2)transform.position + (5.5f * dir), sweepSize);
        float damage = enemy.Transform.GetComponent<Enemy>().enemyData.GetAttackPower() * damageMultiplier;
        damageData.damage = damage;
        warningIndicator.gameObject.SetActive(true);
        warningIndicator.Play(3, (indicator) =>
        {
            enemy.PlayAnim(readyAnimName);
            DealDamage(indicator.transform.position, new Vector2(sweepSize, sweepSize));
            indicator.gameObject.SetActive(false);
            onEnd?.Invoke();
        });

        await UniTask.Delay(TimeSpan.FromSeconds(duration));

        Debug.Log("SweepPattern Execute End");

    }

    void DealDamage(Vector2 origin, Vector2 size)
    {
        Collider2D[] cols = Physics2D.OverlapBoxAll(origin, size, 0);
        foreach (var col in cols)
        {
            if (!col.CompareTag("Player")) continue;

            Character.Instance.TakeDamage(damageData);
        }
    }

    public override void Cancel()
    {
        if (warningIndicator != null)
        {
            warningIndicator.Cancel();
        }

    }
}
