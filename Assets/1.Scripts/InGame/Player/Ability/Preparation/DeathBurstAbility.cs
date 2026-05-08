using UnityEngine;

// [죽음 분열]
// 플레이어 총알로 적을 처치했을 때 사망 위치에서 3/4/5방향으로 AllyBullet을 분열 발사.
// EnemyDeadEvent를 구독하며, cause가 PlayerBullet인 경우에만 발동.
// 분열 총알 데미지는 마력의 damageRate(기본 50%)에 비례.
public class DeathBurstAbility : Ability
{
    static readonly int[] splitCounts = { 3, 4, 5 };
    const float damageRate = 0.5f;

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        if (c < 1) c = 1;
        return $"발사한 총알로 적 처치 시 {splitCounts[c - 1]}방향으로 총알 분열 (데미지 {damageRate * 100:0}%)";
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
        if (e.cause == null || e.cause.GetComponent<PlayerBullet>() == null) return;

        int splits = splitCounts[count - 1];
        float damage = Player.Instance.statMgr.AttackPower * damageRate;
        float angleStep = 360f / splits;
        Vector2 baseDir = Random.insideUnitCircle.normalized;
        for (int i = 0; i < splits; i++)
        {
            Vector2 dir = Quaternion.Euler(0, 0, angleStep * i) * baseDir;
            var bullet = AllyBullet.Instantiate();
            bullet.transform.position = e.position;
            bullet.Shoot(dir, damage);
        }
    }
}
