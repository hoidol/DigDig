using UnityEngine;

// 다중 표적 - 5번에 1번씩 count명의 주변 적에게 추가 발사
public class MultiTargetAbility : Ability
{
    int shotCount;
    const int TRIGGER_EVERY = 5;
    const float SEARCH_RADIUS = 15f;
    public override string GetDescription(int c = -1)
    {
        if (c == -1)
            c = count;
        if (count <= 0)
            c = 1;
        return $"{TRIGGER_EVERY}번 발사 후 주변 다중({c}) 타겟 공격";
    }
    public override void OnEquip(Player player)
    {
        GameEventBus.Subscribe<BulletFiredEvent>(OnBulletFired);
    }

    public override void OnUnequip(Player player)
    {
        shotCount = 0;
        GameEventBus.Unsubscribe<BulletFiredEvent>(OnBulletFired);
    }

    void OnBulletFired(BulletFiredEvent e)
    {
        if (!e.fromPlayer) return;
        shotCount++;
        if (shotCount < TRIGGER_EVERY) return;
        shotCount = 0;

        // 주변 적 탐색 후 가까운 순서로 정렬
        Collider2D[] cols = Physics2D.OverlapCircleAll(
            Player.Instance.transform.position, SEARCH_RADIUS,
            LayerMask.GetMask("Hittable"));

        Vector2 origin = Player.Instance.transform.position;
        System.Array.Sort(cols, (a, b) =>
            ((Vector2)a.transform.position - origin).sqrMagnitude
            .CompareTo(((Vector2)b.transform.position - origin).sqrMagnitude));

        int fired = 0;
        foreach (var col in cols)
        {
            if (fired >= count) break;
            Vector2 dir = ((Vector2)col.transform.position - (Vector2)Player.Instance.attackPoint.position).normalized;
            if (e.dir == dir) continue;
            Player.Instance.Shoot(dir, Vector2.zero);
            fired++;
        }
    }
}
