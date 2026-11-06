// [낙뢰] 천둥 슬라임
// 발사 방향의 첫 적 위치(없으면 사거리 끝)에 낙뢰 → strikeRadius 안 적 최대 maxHitCount기에게 피해
using UnityEngine;

public class ThunderBulletSpec : AllyBulletSpec
{
    public float strikeRadius = 1f;
    public int maxHitCount = 5;
    public float maxDistance = 3f; // 낙뢰 지점을 찾는 최대 거리 (사거리)
    public LayerMask hitLayerMask;

    public ThunderBulletSpec()
    {
        key = "Thunder";
    }
}
