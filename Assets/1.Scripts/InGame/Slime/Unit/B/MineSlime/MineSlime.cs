using System.Collections.Generic;
using UnityEngine;

// [B] 지뢰: 자기 타일이 속한 TileGroup의 enemyRoots 경로 위에 지뢰(Mine)를 던져 설치 (탄을 쏘지 않음)
// 적이 사거리에 없어도 미리 깔아 둔다. 동시에 깔린 지뢰 수는 머지 레벨로 증가(MineSlimeData.maxMineCounts)
// 최대치면 던지지 않고 공격 타이머를 유지 → 지뢰가 터지면 바로 다음 지뢰를 던짐
public class MineSlime : SlimeGrowth1
{
    public Mine minePrefab;

    MineSlimeData mineSlimeData;
    public int maxMineCount;
    public float explodeRadius;
    public int maxHitCount;
    public float mineLifetime;
    public int minesPerThrow; // 한 번에 던지는 지뢰 수 ([각성] 1 > 2)

    // 한 번 공격에 던지는 지뢰 수 (Fire에서 사용)
    public override int ShootCount => minesPerThrow;

    readonly HashSet<Mine> activeMines = new HashSet<Mine>();

    public override void Spawn(Vector2 pos, int lv)
    {
        mineSlimeData = SlimeManager.Instance.GetSlimeData(key) as MineSlimeData;
        base.Spawn(pos, lv);
    }

    public override void InitSlime()
    {
        maxMineCount = mineSlimeData.maxMineCounts.GetByMergeLevel(mergeLevel);
        explodeRadius = mineSlimeData.explodeRadiuses.GetByMergeLevel(mergeLevel);
        maxHitCount = mineSlimeData.maxHitCount;
        mineLifetime = mineSlimeData.mineLifetime;
        minesPerThrow = 1;
        isDragging = false;
        activeMines.Clear(); // 합성 등으로 사라져도 이미 깔린 지뢰는 그대로 남아서 터짐
        base.InitSlime(); // 강화 능력이 값을 바꿀 수 있으므로 기본값 설정 후 호출
    }

    TileGroup GetTileGroup()
    {
        Tile tile = TileManager.Instance.GetTile(this);
        return tile != null ? tile.GetComponentInParent<TileGroup>() : null;
    }

    // 드래그 중에는 던지지 않음 (공격 타이머는 유지 → 내려놓으면 바로 던짐)
    // Slime.isDraging은 타일 선택(StartTarget)에서도 켜지고 안 꺼져서 따로 둠
    bool isDragging;
    public override void StartDrag()
    {
        base.StartDrag();
        isDragging = true;
    }

    public override void EndDrag(Tile t)
    {
        base.EndDrag(t);
        isDragging = false;
    }

    // 타겟 방향은 쓰지 않음 (경로에 설치)
    public override void Fire(Vector2 dir)
    {
        if (isDragging)
            return;

        TileGroup group = GetTileGroup();
        if (group == null || group.enemyRoots == null || group.enemyRoots.Length == 0)
            return;

        // 터졌거나 수명이 다해 풀로 돌아간(또는 다른 슬라임이 재사용한) 지뢰 정리
        activeMines.RemoveWhere(m => m == null || !m.gameObject.activeSelf || !ReferenceEquals(m.Owner, this));

        int count = Mathf.Min(ShootCount, maxMineCount - activeMines.Count);
        if (count <= 0)
            return;

        for (int i = 0; i < count; i++)
        {
            Mine mine = Mine.Spawn(minePrefab);
            mine.Throw(this, transform.position, PickPathPos(group.enemyRoots), AttackPower, explodeRadius, maxHitCount, mineLifetime, targetLayerMask);
            activeMines.Add(mine);
        }
        attackTimer = 0;
    }

    // 탄을 쏘지 않음 (Fire 재정의)
    public override AllyBulletObject GetBullet() => null;

    // 경로(순서대로 이은 선) 위 랜덤 지점 (구간 길이 비례로 골고루). 점이 하나면 그 위치
    static Vector2 PickPathPos(Transform[] points)
    {
        if (points.Length == 1)
            return points[0].position;

        float total = 0f;
        for (int i = 0; i < points.Length - 1; i++)
            total += Vector2.Distance(points[i].position, points[i + 1].position);

        float r = Random.Range(0f, total);
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector2 a = points[i].position;
            Vector2 b = points[i + 1].position;
            float len = Vector2.Distance(a, b);
            if (r <= len && len > 0f)
                return Vector2.Lerp(a, b, r / len);
            r -= len;
        }
        return points[points.Length - 1].position;
    }
}
