using System.Collections.Generic;
using UnityEngine;

// 드래그 가능한 깃발로 높은 공격력의 폭탄을 발사함, 공속 느림 + 25% 확률로 슬로우 (고정, [각성] 50%)
// [A] 폭격
// - 깃발(MortarFlag)은 슬라임 사거리 안에서만 옮길 수 있음. 처음엔 내 TileGroup 적 경로(enemyRoots)에서 가장 가까운 지점
// - 깃발 기준 폭발 범위(boomRange) 안에 적이 있을 때만 발사 → 폭탄은 적이 아니라 깃발 자리에 떨어짐
// - 깃발을 어디 꽂느냐(경로 꺾이는 곳, 철벽이 막는 곳 등)가 배치 포인트
// 프리팹: flagPrefab에 MortarFlag 프리팹 연결
public class MortarSlime : SlimeGrowth1
{
    public MortarFlag flagPrefab;

    MortarBulletSpec mortarBulletSpec;
    MortarSlimeData mortarSlimeData;
    public float boomRange;
    public int maxHitCount;
    public float slowChance; // 0~1. 기본은 데이터(50%, 고정), [각성]이면 100%

    MortarFlag flag;
    SlowOnHitBehavior slowBehavior;
    bool needsFlagPlacement; // 소환 직후엔 아직 타일이 없어서, 타일에 올라간 뒤 깃발 위치를 정함
    Vector2 flagAnchor;      // 플레이어가 꽂은 자리 (실제 깃발 위치 = 이 자리를 사거리 안으로 제한한 곳)

    public Vector2 FlagPos => flag != null ? (Vector2)flag.transform.position : (Vector2)transform.position;
    bool IsFlagReady => flag != null && flag.gameObject.activeSelf && !needsFlagPlacement;

    public override void Spawn(Vector2 pos, int lv)
    {
        mortarSlimeData = SlimeManager.Instance.GetSlimeData(key) as MortarSlimeData;
        needsFlagPlacement = true;
        base.Spawn(pos, lv);
    }

    public override void InitSlime()
    {
        mortarBulletSpec = new MortarBulletSpec();
        boomRange = mortarSlimeData.ranges.GetByMergeLevel(mergeLevel);
        maxHitCount = mortarSlimeData.maxHitCount;
        slowChance = mortarSlimeData.slowChance;

        base.InitSlime(); // 강화 능력이 범위/타격 수/슬로우 확률을 바꿀 수 있으므로 기본값 설정 후 호출

        slowBehavior = new SlowOnHitBehavior(slowChance, mortarSlimeData.slowDuration);

        if (flag == null)
            flag = Instantiate(flagPrefab);
        flag.Init(this);
        flag.gameObject.SetActive(false); // 타일에 올라간 뒤 UpdateFlag에서 위치를 정하고 켬
    }

    // 슬라임·깃발 드래그(SlimeSpawner.Update)가 끝난 뒤 깃발 위치를 맞춤 → 한 프레임도 사거리 밖으로 안 나감
    void LateUpdate()
    {
        UpdateFlag();
    }

    void UpdateFlag()
    {
        if (flag == null)
            return;

        if (needsFlagPlacement)
        {
            Tile tile = TileManager.Instance != null ? TileManager.Instance.GetTile(this) : null;
            if (tile == null)
                return;
            flagAnchor = GetDefaultFlagPos(tile);
            flag.gameObject.SetActive(true);
            needsFlagPlacement = false;
        }

        // 깃발은 항상 사거리 안: 플레이어가 꽂은 자리(flagAnchor)를 기억하고, 보이는 위치는 매 프레임 사거리 안으로 제한
        // → 슬라임을 옮기는 중엔 깃발이 사거리 끝을 따라오고, 원위치로 돌아오거나 사거리가 다시 늘면 꽂은 자리로 복귀
        if (flag.IsDragging)
            flagAnchor = flag.transform.position; // DragTo에서 이미 사거리 안으로 제한됨
        flag.transform.position = ClampFlagPosition(flagAnchor);
        flag.SetRangeRadius(boomRange);
    }

    // 내 TileGroup 적 경로에서 가장 가까운 지점 (경로가 없으면 슬라임 위쪽)
    Vector2 GetDefaultFlagPos(Tile tile)
    {
        Vector2 pos = transform.position;
        TileGroup group = tile.tileGroup;
        Vector2 target = group != null && group.enemyRoots != null && group.enemyRoots.Length > 0
            ? PathLoop.GetNearestPoint(group.enemyRoots, pos)
            : pos + Vector2.up * AttackRange * 0.5f;
        return ClampFlagPosition(target);
    }

    // 깃발은 슬라임 사거리 안으로
    public Vector2 ClampFlagPosition(Vector2 pos)
    {
        Vector2 center = transform.position;
        Vector2 offset = pos - center;
        float range = AttackRange;
        if (offset.magnitude > range)
            offset = offset.normalized * range;
        return center + offset;
    }

    // 타겟 = 깃발 폭발 범위 안의 적 (없으면 발사 안 함)
    public override Transform FindTarget()
    {
        if (!IsFlagReady)
            return null;
        return InGameUtil.FindTarget(FlagPos, boomRange, targetLayerMask, findTargetType);
    }

    public override List<Transform> FindTargets(int count)
    {
        if (!IsFlagReady)
            return new List<Transform>();
        return InGameUtil.FindTargets(FlagPos, boomRange, targetLayerMask, findTargetType, count);
    }

    public override AllyBulletObject GetBullet()
    {
        mortarBulletSpec.boomRange = boomRange;
        mortarBulletSpec.damage = AttackPower;
        mortarBulletSpec.maxHitCount = maxHitCount;
        mortarBulletSpec.targetPos = FlagPos;
        mortarBulletSpec.flightTime = mortarSlimeData.flightTime;
        mortarBulletSpec.arcHeight = mortarSlimeData.arcHeight;
        mortarBulletSpec.hitLayerMask = targetLayerMask;
        mortarBulletSpec.StartBulletSpec();
        mortarBulletSpec.AddBulletBehaviour(slowBehavior);
        return mortarBulletSpec.Instantiate(this);
    }

    public override void StartTarget()
    {
        base.StartTarget();
        if (flag != null)
            flag.ShowRange(true);
    }

    public override void EndTarget()
    {
        base.EndTarget();
        if (flag != null)
            flag.ShowRange(false);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (flag != null)
            flag.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (flag != null)
            Destroy(flag.gameObject);
    }
}
