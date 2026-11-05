using UnityEngine;

// 위험 감수 - 근접 거리 명중 시 공격력 100% 추가 데미지
public class PointBlankAbility : Ability, IBullet
{
    const float CLOSE_RANGE = 2.5f;

    public override void OnEquip(KingSlime player) { }
    public override void OnUnequip(KingSlime player) { }

    public override string GetDescription(bool detail = false)
    {
        return $"{CLOSE_RANGE}m 이내 적중 시 공격력 100% 추가 데미지";
    }

    public void OnBulletFired(KingSlimeBulletObject bullet)
    {
        bullet.AddBulletForce(new PointBlankForce(CLOSE_RANGE));
    }
}

public class PointBlankForce : IBulletForce
{
    readonly float range;

    public PointBlankForce(float range)
    {
        this.range = range;
    }

    public float GetMultiDamage(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        float dist = Vector2.Distance(KingSlime.Instance.transform.position, hit2D.point);
        return dist <= range ? KingSlime.Instance.statMgr.AttackPower : 0f;
    }
}
