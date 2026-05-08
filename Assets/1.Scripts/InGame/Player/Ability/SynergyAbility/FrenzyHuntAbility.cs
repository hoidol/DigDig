using UnityEngine;

// 광란의 사냥 - 적 처치 시 사망 위치에서 3방향 분열탄 발사
public class FrenzyHuntAbility : SynergyAbility
{
    static readonly float damageRatio = 0.5f;
    const int BULLET_COUNT = 3;

    public override void OnEquip(Player player)
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);
    }

    void OnEnemyDead(EnemyDeadEvent e)
    {
        if (e.cause == null)
            return;

        if (e.cause.TryGetComponent<PlayerBullet>(out var b))
        {
            float dmg = Player.Instance.statMgr.AttackPower * damageRatio;
            float angleStep = 360f / BULLET_COUNT;
            for (int i = 0; i < BULLET_COUNT; i++)
            {
                float rad = i * angleStep * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                PlayerBullet pBullet = Player.Instance.Shoot(dir, e.position);
                pBullet.damageData.damage = dmg;
            }
        }


    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"적 처치 시 사망 위치에서 3방향 분열탄 발사 (공격력 {damageRatio * 100:0}%)";
    }
}
