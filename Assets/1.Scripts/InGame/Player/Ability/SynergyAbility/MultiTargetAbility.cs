using UnityEngine;

// 다중 표적 - 매 발사 시 주변 적 최대 3명 추가 공격 (발사 방향과 너무 가까운 적 제외)
public class MultiTargetAbility : Ability
{
    const int MAX_TARGETS = 2;
    const float SEARCH_RADIUS = 15f;
    const float EXCLUDE_ANGLE = 15f;

    public override string GetDescription(int c = -1, bool detail = false)
    {
        return $"매 발사 시 주변 적 최대 {MAX_TARGETS}명 추가 공격";
    }

    public override void OnEquip(Player player)
    {
        GameEventBus.Subscribe<BulletFiredEvent>(OnBulletFired);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<BulletFiredEvent>(OnBulletFired);
    }

    void OnBulletFired(BulletFiredEvent e)
    {
        if (!e.fromPlayer) return;

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
            if (fired >= MAX_TARGETS) break;
            Vector2 dir = ((Vector2)col.transform.position - (Vector2)Player.Instance.attackPoint.position).normalized;
            if (Vector2.Angle(e.dir, dir) < EXCLUDE_ANGLE) continue;
            Player.Instance.Shoot(dir, Vector2.zero);
            fired++;
        }
    }
}
