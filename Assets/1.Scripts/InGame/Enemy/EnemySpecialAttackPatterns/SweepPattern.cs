using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

// 크게 휘두르기 패턴
// 플레이어 방향으로 부채꼴 범위 공격
// 페이즈가 높을수록 범위와 피해 증가
// 가까워졌을때 발동되야 재밌는데.. 
public class SweepPattern : EnemySpecialAttackPattern
{
    [SerializeField] SweepWarningIndicator sweepIndicator;
    [SerializeField] float damageMultiplier = 1.5f;
    [SerializeField] float sweepSize = 5;
    DamageData damageData = new DamageData();
    public async override UniTask Execute(IEnemySpecialAttackPattern enemy, Action onEnd)
    {
        await base.Execute(enemy, onEnd);

        float damage = enemy.Transform.GetComponent<Enemy>().enemyData.GetAttackPower() * damageMultiplier;

        damageData.damage = damage;
        sweepIndicator.gameObject.SetActive(true);
        sweepIndicator.Play(3, () =>
        {
            DealDamage(sweepIndicator.transform.position, new Vector2(sweepSize, sweepSize));
            onEnd?.Invoke();
        });
    }

    void DealDamage(Vector2 origin, Vector2 size)
    {
        Collider2D[] cols = Physics2D.OverlapBoxAll(origin, size, 0);
        foreach (var col in cols)
        {
            if (!col.CompareTag("Player")) continue;

            Player.Instance.TakeDamage(damageData);
        }
    }

    public override void Cancel()
    {
        sweepIndicator.gameObject.SetActive(false);
        sweepIndicator.Cancel();
    }
}
