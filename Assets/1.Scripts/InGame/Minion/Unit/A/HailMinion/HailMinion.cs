using System.Collections.Generic;
using UnityEngine;

// [A] 빙석: 탄을 쏘지 않고, 내 타일이 속한 TileGroup의 적 경로(enemyRoots)를 순서대로 빙빙 도는 눈(HailStone)을 소환
// - 경로는 enemyRoots[0] → [1] → … → 마지막 → 다시 [0] (닫힌 고리로 계속 순환)
// - 눈 hailCount개는 경로 위에 같은 간격으로 배치되어 함께 이동
// - 이동 속도 = moveSpeed × 공속 / ATTACK_SPEED_PER_SECOND (공속 버프가 속도에 반영, Orbit과 같은 규칙)
// - 닿은 적: 피해(공격력) + iceChance%(머지 레벨별) 확률로 얼음
// - 슬라임을 다른 그룹으로 옮기면 그 그룹 경로로 바뀜. 드래그 중에는 눈 숨김
// - [각성] (진화) 눈 EVOLVE_HAIL_COUNT개
// 프리팹: hailPrefab에 HailStone 프리팹 연결
public class HailMinion : MinionGrowth1, IRotationDirectionUnit
{
    // 경로를 도는 방향 설정 (설정 버튼). 타겟을 노리지 않으므로 타겟 방식 버튼은 숨김
    public RotationDirection RotationDirection => rotationDirection;
    public void SetRotationDirection(RotationDirection direction) => rotationDirection = direction;
    public override bool UseFindTarget => false;

    public const int EVOLVE_HAIL_COUNT = 2;

    public HailStone hailPrefab;

    HailMinionData hailMinionData;

    public int hailCount;
    public float iceChance;
    public RotationDirection rotationDirection; // 기본값은 데이터, 설정 버튼으로 변경

    readonly List<HailStone> hails = new List<HailStone>();
    float travel;          // 경로 위 진행 거리 (첫 번째 눈 기준)
    bool isDragging;

    public override void Spawn(Vector2 pos, int lv)
    {
        hailMinionData = MinionManager.Instance.GetMinionData(key) as HailMinionData;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        hailCount = hailMinionData.hailCount;
        iceChance = hailMinionData.iceChances.GetByMergeLevel(mergeLevel);
        rotationDirection = hailMinionData.rotationDirection;
        isDragging = false;
        travel = 0f;
        base.InitMinion();
        PrepareHails();
    }

    protected override void OnEvolve()
    {
        hailCount = Mathf.Max(hailCount, EVOLVE_HAIL_COUNT);
        PrepareHails();
    }

    // 개수만큼 눈 준비 (남는 건 끔)
    void PrepareHails()
    {
        while (hails.Count < hailCount)
            hails.Add(Instantiate(hailPrefab, transform));
        for (int i = 0; i < hails.Count; i++)
        {
            hails[i].Init(this, hailMinionData.hitCooldown);
            hails[i].iceChance = iceChance;
            hails[i].iceDuration = hailMinionData.iceDuration;
            hails[i].gameObject.SetActive(false); // 경로 위치를 정한 뒤 UpdateHails에서 켬
        }
    }

    public float MoveSpeed => hailMinionData.moveSpeed * AttackSpeed / GameSetting.ATTACK_SPEED_PER_SECOND;

    public override void Update()
    {
        base.Update();
        UpdateHails();
    }

    void UpdateHails()
    {
        Transform[] path = GetPath();
        float total = PathLoop.GetLength(path);
        bool show = !isDragging && total > 0f;

        for (int i = 0; i < hails.Count; i++)
        {
            bool active = show && i < hailCount;
            if (hails[i].gameObject.activeSelf != active)
                hails[i].gameObject.SetActive(active);
        }
        if (!show)
            return;

        if (GameManager.Instance == null || GameManager.Instance.isPlaying)
            travel = Mathf.Repeat(travel + PathLoop.GetTravelSign(path, rotationDirection) * MoveSpeed * Time.deltaTime, total);

        float gap = total / hailCount;
        float damage = AttackPower;
        for (int i = 0; i < hailCount && i < hails.Count; i++)
        {
            hails[i].damage = damage;
            hails[i].transform.position = PathLoop.GetPoint(path, total, travel + gap * i);
        }
    }

    Transform[] GetPath()
    {
        if (TileManager.Instance == null)
            return null;
        Tile tile = TileManager.Instance.GetTile(this);
        TileGroup group = tile != null ? tile.tileGroup : null;
        if (group == null || group.enemyRoots == null || group.enemyRoots.Length < 2)
            return null;
        return group.enemyRoots;
    }

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

    // 탄을 쏘지 않음 (눈이 피해 담당)
    public override void Fire(Vector2 dir) { }
    public override AllyBulletObject GetBullet() => null;
}
