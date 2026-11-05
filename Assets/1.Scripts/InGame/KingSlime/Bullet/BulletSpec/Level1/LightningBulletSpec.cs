// [낙뢰탄]
// 적중 시 플레이어 주변에서 가장 가까운 적/광석에 낙뢰 (ThunderItem 동일 방식)
using UnityEngine;
[System.Serializable]
public class LightningBulletSpec : AllyBulletSpec
{
    public float initSearchRadius = 6f;
    public float searchRadius = 3f;
    public int lightningCount;
    public LayerMask hitLayerMask;

    public LightningBulletSpec()
    {
        key = "Lightning";
    }

}
