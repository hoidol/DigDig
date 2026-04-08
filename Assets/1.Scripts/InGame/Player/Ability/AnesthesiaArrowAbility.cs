using UnityEngine;

// 마취 화살 - 적중 시 확률로 기절
public class AnesthesiaArrowAbility : Ability, IBulletItem
{
    public float[] stunChances = { 0.15f, 0.2f, 0.3f };   // count당 15%
    public float[] stunDurations = { 2f, 2.5f, 3f };   // count당 15%
    public override string GetDescription(int c = -1)
    {
        if (c == -1)
            c = count;
        if (count <= 0)
            c = 1;
        return $"{stunChances[c - 1] * 100}% 확률로 {stunDurations[c - 1]}초 간 마비\n(보스 제외)";
    }

    public void OnBulletFired(PlayerBullet bullet)
    {
        bullet.AddBehavior(new StunBulletBehavior(stunChances[count - 1], stunDurations[count - 1]));
    }

    public override void OnUnequip(Player player) { }
}

public class StunBulletBehavior : IBulletBehavior
{
    readonly float chance;
    readonly float duration;
    public StunBulletBehavior(float chance, float d)
    {
        this.chance = chance;
        duration = d;
    }

    public bool OnHit(BulletBase bullet, IHittable hit, RaycastHit2D hit2D)
    {
        if (Random.value < chance && hit is Enemy enemy)
            enemy.GetComponent<StatusEffectHandler>()?.Apply(new StunEffect(duration));
        return true;
    }

    public void OnMove(BulletBase bullet) { }
    public void Merge(IBulletBehavior other) { }
}
