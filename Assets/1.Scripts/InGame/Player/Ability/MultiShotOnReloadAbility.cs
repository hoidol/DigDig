using UnityEngine;

// 멀티샷 - 장전 완료 후 첫 발은 멀티샷
public class MultiShotOnReloadAbility : Ability, IAttackItem
{
    bool firstShot;
    public override string GetDescription(int c = -1)
    {
        return $"장전 후 첫발 멀티샷";
    }

    public override void OnEquip(Player player)
    {
        GameEventBus.Subscribe<ReloadEndEvent>(OnReloadEnd);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<ReloadEndEvent>(OnReloadEnd);
        firstShot = false;
    }

    void OnReloadEnd(ReloadEndEvent e) => firstShot = true;

    public void OnAttack(Player player, Vector2 dir)
    {
        if (!firstShot) return;
        firstShot = false;

        int extra = count + 1; // count=1→2발 추가(총3발), count=2→3발 추가(총4발)
        float totalAngle = 30f;
        float angleStep = totalAngle / extra;
        float baseAngle = Vector2.SignedAngle(Vector2.right, dir);

        // 반 스텝 오프셋 → dir과 절대 겹치지 않음
        for (int i = 0; i < extra; i++)
        {
            float angle = baseAngle - totalAngle / 2f + angleStep * 0.5f + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 spreadDir = new(Mathf.Cos(rad), Mathf.Sin(rad));
            Player.Instance.Shoot(spreadDir);
        }
    }
}
