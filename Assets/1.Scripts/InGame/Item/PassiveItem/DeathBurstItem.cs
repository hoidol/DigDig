using UnityEngine;

public class DeathBurstItem : Item
{
    public int splitCount = 8;
    public float damageRate = 0.5f; // 기본 공격력의 50%

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
        float damage = Player.Instance.playerStatMgr.AttackPower * damageRate;
        float angleStep = 360f / splitCount;

        for (int i = 0; i < splitCount; i++)
        {
            Vector2 dir = Quaternion.Euler(0, 0, angleStep * i) * Vector2.right;
            var bullet = PlayerBullet.Instantiate();
            bullet.transform.position = e.position;
            bullet.Shoot(dir, damage);
        }
    }
}
