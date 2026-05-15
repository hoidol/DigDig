using UnityEngine;

// 위험 감수 - 근접 거리에서 명중 시 추가 데미지
public class PointBlankAbility : Ability, IBulletItem
{
    static readonly float[] bonuses = { 40f, 50f, 60f, 80f, 100f };
    const float CLOSE_RANGE = 2.5f;

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        bullet.AddBulletForce(new PointBlankForce(bonuses[count - 1], CLOSE_RANGE));
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"{CLOSE_RANGE}m 이내 적중 시 {bonuses[c - 1]:0} 추가 데미지";
    }
}

public class PointBlankForce : IBulletForce
{
    readonly float bonus;
    readonly float range;

    public PointBlankForce(float bonus, float range)
    {
        this.bonus = bonus;
        this.range = range;
    }

    public float GetMultiDamage(BulletBase bullet, IHittable hit, RaycastHit2D hit2D)
    {
        float dist = Vector2.Distance(Player.Instance.transform.position, hit2D.point);
        if (dist <= range)
            return bonus;
        return 0;
    }
}
