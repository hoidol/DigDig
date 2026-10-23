using UnityEngine;

// 스턴: 적중 시 chance% 확률로 duration초 스턴 (보스 제외)
public class StunTileItem : TileItem //타일 위에 설치되는 아이템
{
    public float chance = 2f;
    public float duration = 0.3f;

    IBulletBehavior hitBehavior;
    public override IBulletBehavior HitBehavior => hitBehavior ??= new ChanceStunBehavior(chance, duration);
}

public class ChanceStunBehavior : IBulletBehavior
{
    public int ApplyOrder => -1; // 관통 등 다음 처리를 끊는 behavior보다 먼저
    readonly float chance;
    readonly float duration;

    public ChanceStunBehavior(float chance, float duration)
    {
        this.chance = chance;
        this.duration = duration;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        if (hit is Boss || hit.CurHp <= 0)
            return true;
        if (Random.Range(0f, 100f) < chance)
            hit.ApplyStatusEffect(new StunEffect(duration));
        return true;
    }
}
