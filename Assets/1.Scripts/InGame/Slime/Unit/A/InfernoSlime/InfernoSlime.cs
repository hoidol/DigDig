using System.Linq;
using UnityEngine;

// [A] 지옥: 탄을 쏘지 않고, 자기 TileGroup의 enemyRoots 중 슬라임과 가장 가까운 2개 사이에 지옥불(Hellfire) 생성
// 공격 주기(공속)마다 지옥불 안에 있는 적 전부에게 AttackPower 피해. 영역을 벗어나면 바로 피해 없음
// TileGroup 3*3의 가운데 칸이면 지옥불 없음 (위치가 지옥불을 결정 → 배치 포인트)
// [각성] (진화) 지옥불 피해를 입은 적에게 화상(FlameEffect, 초당 AttackPower × burnDpsRatio, burnDuration초)
public class InfernoSlime : SlimeGrowth1
{
    public Hellfire hellfirePrefab;
    Hellfire hellfire;

    InfernoSlimeData infernoSlimeData;
    readonly AllyUnitDamageData damageData = new AllyUnitDamageData();

    bool isDragging;

    public override void Spawn(Vector2 pos, int lv)
    {
        infernoSlimeData = SlimeManager.Instance.GetSlimeData(key) as InfernoSlimeData;
        base.Spawn(pos, lv);
    }

    public override void InitSlime()
    {
        isDragging = false;
        base.InitSlime();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (hellfire != null)
            hellfire.Hide();
    }

    void OnDestroy()
    {
        if (hellfire != null)
            Destroy(hellfire.gameObject);
    }

    public override void StartDrag()
    {
        base.StartDrag();
        isDragging = true;
        if (hellfire != null)
            hellfire.Hide();
    }

    public override void EndDrag(Tile t)
    {
        base.EndDrag(t);
        isDragging = false;
        RefreshHellfire();
    }

    // 지금 타일 기준으로 지옥불 위치 갱신. 만들 수 없으면(타일 없음/가운데 칸/경로 점 부족) 숨김
    const float EMBER_RADIUS = 0.8f; // 잔불의 화로 불길 반경 (지옥불은 원형 범위가 없어서 고정)

    bool RefreshHellfire()
    {
        Tile tile = TileManager.Instance.GetTile(this);
        TileGroup group = tile != null ? tile.GetComponentInParent<TileGroup>() : null;
        // 측량사의 나침반: 옆 그룹 경로까지 후보 → 가장 가까운 경로를 쓰는 그룹
        if (group != null && !IsCenterTile(tile))
            group = SurveyCompassAuthority.GetPathGroups(group)
                .Where(g => g.enemyRoots != null && g.enemyRoots.Length >= 2)
                .OrderBy(g => g.enemyRoots.Where(r => r != null).Min(r => ((Vector2)r.position - (Vector2)tile.transform.position).sqrMagnitude))
                .FirstOrDefault() ?? group;
        if (group == null || IsCenterTile(tile) || group.enemyRoots == null || group.enemyRoots.Length < 2)
        {
            if (hellfire != null)
                hellfire.Hide();
            return false;
        }

        FindNearestTwo(group.enemyRoots, tile.transform.position, out Transform a, out Transform b);
        if (hellfire == null)
            hellfire = Instantiate(hellfirePrefab);
        hellfire.Set(a.position, b.position); // 크기는 프리팹 그대로, 세로면 z 90
        return true;
    }

    // TileGroup 3*3이 2*2로 놓인 6*6 격자 → 그룹 안 가운데 칸 = index가 둘 다 3으로 나눠 1
    static bool IsCenterTile(Tile tile)
    {
        return tile.index.x % 3 == 1 && tile.index.y % 3 == 1;
    }

    static void FindNearestTwo(Transform[] roots, Vector2 pos, out Transform first, out Transform second)
    {
        first = null;
        second = null;
        float d1 = float.MaxValue, d2 = float.MaxValue;
        foreach (Transform r in roots)
        {
            if (r == null) continue;
            float d = ((Vector2)r.position - pos).sqrMagnitude;
            if (d < d1)
            {
                second = first; d2 = d1;
                first = r; d1 = d;
            }
            else if (d < d2)
            {
                second = r; d2 = d;
            }
        }
    }

    // 타겟 방향은 쓰지 않음. 공격 주기마다 지옥불 안 적 전부에게 피해
    public override void Fire(Vector2 dir)
    {
        if (isDragging || !RefreshHellfire())
            return;

        float damage = AttackPower;
        damageData.Init(this);
        damageData.attackType = AttackType;

        var hellfireTargets = hellfire.GetEnemies(targetLayerMask).ToList();
        if (hellfireTargets.Count > 0)
            EmberBrazierAuthority.OnZoneDamage(hellfireTargets[UnityEngine.Random.Range(0, hellfireTargets.Count)].Transform.position, EMBER_RADIUS, damage, targetLayerMask);
        foreach (IHittable hit in hellfireTargets)
        {
            damageData.damage = damage;
            AccumulateDamage(damage);
            hit.TakeDamage(damageData);

            if (isEvolved && hit is Enemy enemy && enemy.CurHp > 0)
                enemy.ApplyStatusEffect(new FlameEffect(infernoSlimeData.burnDuration, AttackPower * infernoSlimeData.burnDpsRatio));
        }
        attackTimer = 0; // 안에 적이 없어도 주기는 그대로 돈다
    }

    // 탄을 쏘지 않음 (Fire 재정의)
    public override AllyBulletObject GetBullet() => null;
}
