// [낙뢰탄]
// 적중 시 플레이어 주변에서 가장 가까운 적/광석에 낙뢰 (ThunderItem 동일 방식)
using UnityEngine;

public class LightningBulletSpec : BulletSpec
{
    public float initSearchRadius = 6f;
    public float searchRadius = 3f;
    public int lightningCount;
    public float damage = 1f; // 공격력의 100%
    public LayerMask hitLayerMask;

    public LightningBulletSpec()
    {
        key = "Lightning";        
    }


}
