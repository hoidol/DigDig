using UnityEngine;
public class ChainPickaxeBehavior : IBulletBehavior
{
    public int ApplyOrder => 10;
    
    float chance = 60f;
    int chainCount = 3;
    LayerMask layerMask = LayerMask.GetMask("Hittable"); 
    float chainRadius =4;
    float damage;
    public ChainPickaxeBehavior(int count) { chainCount = count; }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        
        if(!hit.CanHit() && hit.CurHp <= 0)
        {
             if (Random.Range(0f, 100f) > chance) return false;

        Collider2D[] cols = Physics2D.OverlapCircleAll(
            hit2D.point, chainRadius, layerMask);

        var candidates = new System.Collections.Generic.List<(Stone stone, float dist)>();
        foreach (var col in cols)
        {
            if (!col.TryGetComponent(out Stone stone)) continue;            
            float dist = Vector2.Distance(hit2D.point, stone.transform.position);
            candidates.Add((stone, dist));
        }
        candidates.Sort((a, b) => a.dist.CompareTo(b.dist));
        // float damage = Character.Instance.statMgr.AttackPower/2f;
        int hitCount = Mathf.Min(chainCount, candidates.Count);
        for (int i = 0; i < hitCount; i++)
        {
            candidates[i].stone.TakeDamage(new DamageData { damage = damage });
            EffectManager.Instance.Play(EffectType.Spark, candidates[i].stone.transform.position);
        }
        }
        return false;
    }
}
