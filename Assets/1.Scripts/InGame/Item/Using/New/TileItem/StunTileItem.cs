using UnityEngine;

// 스턴: 적중 시 chance% 확률로 duration초 스턴 (보스 제외)
public class StunTileItem : TileItem //타일 위에 설치되는 아이템
{
    public float chance = 2f;
    public float duration = 0.3f;

    protected override void ApplyLevelValue(float v)
    {
        chance = v; // 레벨 수치 = 스턴 확률
        hitBehavior = null; // 확률을 들고 있는 behavior 다시 생성
    }

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
        if (hit.CurHp <= 0) // 엘리트/보스도 걸림 (StatusEffectHandler에서 시간 절반)
            return true;
        if (Random.Range(0f, 100f) < chance)
            hit.ApplyStatusEffect(new StunEffect(duration));
        return true;
    }
}
