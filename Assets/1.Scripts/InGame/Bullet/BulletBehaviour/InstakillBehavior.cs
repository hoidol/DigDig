using UnityEngine;

public class InstakillBehavior : IBulletBehavior
{
    float chance;

    public InstakillBehavior(float chance)
    {
        this.chance = chance;
    }

    public bool OnHit(BulletBase bullet, IHittable hit, RaycastHit2D hit2d)
    {
        if (Random.value <= chance)
        {
            // 보스/엘리트 제외 태그 체크
            Component comp = hit as Component;
            if (comp != null && !comp.CompareTag("Boss") && !comp.CompareTag("Elite"))
            {
                hit.TakeDamage(float.MaxValue);
            }
        }
        return true;
    }

    public void OnMove(BulletBase bullet) { }

    public void Merge(IBulletBehavior other)
    {
        float otherChance = ((InstakillBehavior)other).chance;
        if (otherChance > chance) chance = otherChance;
    }
}
