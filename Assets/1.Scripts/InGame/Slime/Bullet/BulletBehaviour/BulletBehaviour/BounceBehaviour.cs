using UnityEngine;

// (2026-11-21) 바운드 = 연쇄: 적에게 맞으면 반사각으로 튕기지 않고, 맞은 적 주변(CHAIN_RANGE)에서
// 아직 안 맞은 가장 가까운 적을 쫓아가 타격 (Huntress 문글레이브 방식). 다음 적이 없으면 사라짐.
// 남은 횟수·맞은 적 목록은 탄(AllyBulletObject)에 둠 → 연발 등으로 같은 behavior를 공유해도 탄마다 따로 셈.
public class BounceBehavior : IBulletBehavior
{
    public const float CHAIN_RANGE = 3f;
    public int ApplyOrder => 0;
    readonly int count;
    readonly float multi;

    public BounceBehavior(int count, float mul)
    {
        this.count = count;
        multi = mul;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        if (bullet is not AllyBulletObject ally)
            return true;

        bullet.damageMultiplier *= multi;
        ally.chainHitSet.Add(hit);
        if (ally.chainCount >= count)
            return true;

        IHittable next = FindNextTarget(ally, hit.Transform.position);
        if (next == null)
            return true;

        ally.chainCount++;
        ally.SetChainTarget(next);
        return false;
    }

    static IHittable FindNextTarget(AllyBulletObject bullet, Vector2 from)
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(from, CHAIN_RANGE, bullet.hitLayerMask);
        IHittable best = null;
        float bestDist = float.MaxValue;
        foreach (Collider2D col in cols)
        {
            IHittable h = col.GetComponent<IHittable>();
            if (h == null || h.CurHp <= 0 || bullet.chainHitSet.Contains(h))
                continue;
            float dist = ((Vector2)h.Transform.position - from).sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                best = h;
            }
        }
        return best;
    }
}
