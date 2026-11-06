using UnityEngine;

// [A] 지옥: 탄을 쏘지 않고, 자기 TileGroup의 enemyRoots 중 슬라임과 가장 가까운 2개 사이에 지옥불(Hellfire) 생성
// 공격 주기(공속)마다 지옥불 안에 있는 적 전부에게 AttackPower × damageMul 피해. 영역을 벗어나면 바로 피해 없음
// TileGroup 3*3의 가운데 칸이면 지옥불 없음 (위치가 지옥불을 결정 → 배치 포인트)
// [각성] 지옥불 피해를 입은 적에게 화상(FlameEffect) 부여
public class InfernoSlime : SlimeGrowth1
{
    public Hellfire hellfirePrefab;
    Hellfire hellfire;

    InfernoSlimeData infernoSlimeData;
    readonly AllyUnitDamageData damageData = new AllyUnitDamageData();

    // 강화 능력이 바꿀 수 있음 (InitSlime에서 base 호출 전에 기본값 설정)
    public float damageMul;
    public bool applyBurn;      // [각성]
    public float burnDuration;
    public float burnDpsRatio;

    bool isDragging;

    public override void Spawn(Vector2 pos, int lv)
    {
        infernoSlimeData = SlimeManager.Instance.GetSlimeData(key) as InfernoSlimeData;
        base.Spawn(pos, lv);
    }

    public override void InitSlime()
    {
        damageMul = 1f;
        applyBurn = false;
        burnDuration = infernoSlimeData.burnDuration;
        burnDpsRatio = infernoSlimeData.burnDpsRatio;
        isDragging = false;
        base.InitSlime(); // 강화 능력이 값을 바꿀 수 있으므로 기본값 설정 후 호출
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
    bool RefreshHellfire()
    {
        Tile tile = TileManager.Instance.GetTile(this);
        TileGroup group = tile != null ? tile.GetComponentInParent<TileGroup>() : null;
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

        float damage = AttackPower * damageMul;
        damageData.Init(this);
        damageData.attackType = AttackType;

        foreach (IHittable hit in hellfire.GetEnemies(targetLayerMask))
        {
            damageData.damage = damage;
            AccumulateDamage(damage);
            hit.TakeDamage(damageData);

            if (applyBurn && hit is Enemy enemy && enemy.CurHp > 0)
                enemy.ApplyStatusEffect(new FlameEffect(burnDuration, AttackPower * burnDpsRatio));
        }
        attackTimer = 0; // 안에 적이 없어도 주기는 그대로 돈다
    }

    // 탄을 쏘지 않음 (Fire 재정의)
    public override AllyBulletObject GetBullet() => null;
}
