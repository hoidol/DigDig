using UnityEngine;

// 둔화: 발사 시 chance% 확률로 그 탄에 슬로우(duration초)를 붙임. 붙은 탄은 맞는 적마다 슬로우
public class SlowTileItem : TileItem //타일 위에 설치되는 아이템
{
    public float chance = 30f;
    public float duration = 2f;

    SlowOnHitBehavior slowBehavior;

    // 읽을 때마다 확률 판정 → 탄은 발사 시(OnFired) 1번, 탄이 없는 Orbit은 적중마다 1번
    public override IBulletBehavior HitBehavior =>
        Random.Range(0f, 100f) < chance ? slowBehavior ??= new SlowOnHitBehavior(1f, duration) : null;
}
