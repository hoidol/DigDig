using System;
using UnityEngine;

// (2026-11-21) 킹 패시브용 적중 훅: 킹 탄이 적에게 맞을 때마다 onHit (피해 계산 뒤라 DamageData.lastHitCrit 사용 가능)
// 탄은 소멸 여부에 관여하지 않음 (관통 등 다른 동작이 정함)
public class KingHitBehavior : IBulletBehavior
{
    readonly Action<AllyBulletObject, Enemy> onHit;
    public int ApplyOrder => 0;

    public KingHitBehavior(Action<AllyBulletObject, Enemy> onHit)
    {
        this.onHit = onHit;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        if (hit is Enemy enemy && bullet is AllyBulletObject ally)
            onHit?.Invoke(ally, enemy);
        return true;
    }
}
