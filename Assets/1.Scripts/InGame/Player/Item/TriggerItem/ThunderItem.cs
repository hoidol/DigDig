using UnityEngine;

// [번개]
// 쿨타임마다 플레이어 주변에 번개 낙하. 가까운 적 > 가까운 광석 > 빈 곳 순으로 타격.
// 낙하 지점 주변 AOE 데미지 (마력 기반).
public class ThunderItem : TriggerItem
{
    public float searchRadius = 8f;
    public float strikeRadius = 1.5f;
    public LayerMask hittableLayer;

    static readonly float[] damageRates = { 0.8f, 1.2f, 1.8f };

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        float rate = damageRates[Mathf.Clamp(c - 1, 0, damageRates.Length - 1)];
        return $"쿨타임마다 번개 낙하 (마력의 {rate * 100:0}% 데미지, 가까운 적/광석 우선 타격)";
    }

    public override void OnTrigger()
    {
        base.OnTrigger();
        Strike(FindTarget());
    }

    Vector2 FindTarget()
    {
        Vector2 playerPos = Player.Instance.transform.position;
        Collider2D[] cols = Physics2D.OverlapCircleAll(playerPos, searchRadius, hittableLayer);

        Enemy nearestEnemy = null;
        OreStone nearestOre = null;
        float enemyDistSq = float.MaxValue;
        float oreDistSq = float.MaxValue;

        foreach (var col in cols)
        {
            float distSq = ((Vector2)col.transform.position - playerPos).sqrMagnitude;
            if (col.TryGetComponent(out Enemy enemy) && enemy.CurHp > 0)
            {
                if (distSq < enemyDistSq) { enemyDistSq = distSq; nearestEnemy = enemy; }
            }
            else if (col.TryGetComponent(out OreStone ore) && ore.curHp > 0)
            {
                if (distSq < oreDistSq) { oreDistSq = distSq; nearestOre = ore; }
            }
        }

        if (nearestEnemy != null) return nearestEnemy.transform.position;
        if (nearestOre != null) return nearestOre.transform.position;

        return playerPos + Random.insideUnitCircle.normalized * (searchRadius * Random.Range(0.3f, 1f));
    }

    void Strike(Vector2 pos)
    {
        float damage = Player.Instance.statMgr.MagicPower
            * damageRates[Mathf.Clamp(count - 1, 0, damageRates.Length - 1)];

        AOEUtil.DamageEnemies(pos, strikeRadius, damage, hittableLayer);
        EffectManager.Instance.Play(EffectType.Spark, pos);
    }
}
