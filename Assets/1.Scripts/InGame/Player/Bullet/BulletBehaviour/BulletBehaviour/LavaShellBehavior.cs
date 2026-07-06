using UnityEngine;

// 마지막 튕김 지점에 용암 지대 생성
public class LavaShellBehavior : IBulletBehavior
{
    int remaining;
    readonly float lavaRadius;
    readonly float lavaDamageRate;
    readonly float lavaDuration;

    public LavaShellBehavior(int bounceCount, float lavaRadius, float lavaDamageRate, float lavaDuration)
    {
        remaining = bounceCount;
        this.lavaRadius = lavaRadius;
        this.lavaDamageRate = lavaDamageRate;
        this.lavaDuration = lavaDuration;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        bullet.damageMultiplier *= Player.Instance.statMgr.AmmoEfficiency;

        if (remaining-- <= 0)
        {
            SpawnLavaZone(hit2D.point, bullet.hitLayerMask);
            return true;
        }

        bullet.Bounce(hit2D);
        return false;
    }

    void SpawnLavaZone(Vector2 pos, LayerMask layer)
    {
        GameObject zoneObj = new GameObject("LavaZone");
        zoneObj.transform.position = pos;
        LavaZone zone = zoneObj.AddComponent<LavaZone>();
        zone.Init(lavaRadius, layer, lavaDamageRate, lavaDuration);
    }
}
