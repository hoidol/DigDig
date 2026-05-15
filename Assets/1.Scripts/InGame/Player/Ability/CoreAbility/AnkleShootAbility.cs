using UnityEngine;

// 앵클샷 - 적중 시 확률로 슬로우
public class AnkleShootAbility : Ability, IBulletItem
{
    static readonly float[] chances   = { 0.25f, 0.30f, 0.40f, 0.45f, 0.55f };
    static readonly float[] durations = { 2f, 2.5f, 3f, 3.5f, 4f };

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        bullet.AddBehavior(new SlowOnHitBehavior(chances[count - 1], durations[count - 1]));
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"{chances[c - 1] * 100:0}% 확률로 {durations[c - 1]}초 슬로우";
    }
}

public class SlowOnHitBehavior : IBulletBehavior
{
    readonly float chance;
    readonly float duration;

    public SlowOnHitBehavior(float chance, float duration)
    {
        this.chance   = chance;
        this.duration = duration;
    }

    public bool OnHit(BulletBase bullet, IHittable hit, RaycastHit2D hit2D)
    {
        if (Random.value < chance && hit is Enemy enemy)
        {
            var handler = enemy.GetComponent<StatusEffectHandler>();
            if (handler != null) handler.Apply(new SlowEffect(duration));
        }
        return true;
    }

    public void OnMove(BulletBase bullet) { }
    public void Merge(IBulletBehavior other) { }
}
