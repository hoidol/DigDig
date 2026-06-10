using UnityEngine;

// 죽음 분열 - 플레이어 총알로 적 처치 시 3방향 분열 발사 (데미지 50%)
public class DeathBurstAbility : Ability
{
    const int SPLIT_COUNT = 3;
    const float DAMAGE_RATE = 0.5f;

    public override string GetDescription(bool detail = false)
    {
        return $"발사한 총알로 적 처치 시 {SPLIT_COUNT}방향으로 총알 분열 (데미지 {DAMAGE_RATE * 100:0}%)";
    }

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
        // if (e.cause == null || e.cause.GetComponent<PlayerBulletObject>() == null) return;

        // float damage = Player.Instance.statMgr.AttackPower * DAMAGE_RATE;
        // float angleStep = 360f / SPLIT_COUNT;
        // Vector2 baseDir = Random.insideUnitCircle.normalized;
        // for (int i = 0; i < SPLIT_COUNT; i++)
        // {
        //     Vector2 dir = Quaternion.Euler(0, 0, angleStep * i) * baseDir;
        //     var bullet = AllyBullet.Instantiate();
        //     bullet.transform.position = e.position;
        //     bullet.Shoot(dir, damage);
        // }
    }
}
