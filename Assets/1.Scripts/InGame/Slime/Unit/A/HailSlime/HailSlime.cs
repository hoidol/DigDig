using System.Collections.Generic;
using UnityEngine;

// [A] 빙석: 탄을 쏘지 않고, 내 타일이 속한 TileGroup의 적 경로(enemyRoots)를 순서대로 빙빙 도는 눈(HailStone)을 소환
// - 경로는 enemyRoots[0] → [1] → … → 마지막 → 다시 [0] (닫힌 고리로 계속 순환)
// - 눈 hailCount개(각성 2개)는 경로 위에 같은 간격으로 배치되어 함께 이동
// - 이동 속도 = moveSpeed × 공속 / ATTACK_SPEED_PER_SECOND (공속 강화·버프가 속도에 반영, Orbit과 같은 규칙)
// - 닿은 적: 피해(공격력) + iceChance%(머지 레벨별) 확률로 얼음
// - 슬라임을 다른 그룹으로 옮기면 그 그룹 경로로 바뀜. 드래그 중에는 눈 숨김
// 프리팹: hailPrefab에 HailStone 프리팹 연결
public class HailSlime : SlimeGrowth1
{
    public HailStone hailPrefab;

    HailSlimeData hailSlimeData;

    // 강화 능력이 바꿀 수 있음 (InitSlime에서 base 호출 전에 기본값 설정)
    public int hailCount;
    public float moveSpeedMul;
    public float iceChance;
    public RotationDirection rotationDirection; // 기본값은 데이터, 강화 등으로 바꿀 수 있음

    readonly List<HailStone> hails = new List<HailStone>();
    float travel;          // 경로 위 진행 거리 (첫 번째 눈 기준)
    bool isDragging;

    public override void Spawn(Vector2 pos, int lv)
    {
        hailSlimeData = SlimeManager.Instance.GetSlimeData(key) as HailSlimeData;
        base.Spawn(pos, lv);
    }

    public override void InitSlime()
    {
        hailCount = hailSlimeData.hailCount;
        moveSpeedMul = 1f;
        iceChance = hailSlimeData.iceChances.GetByMergeLevel(mergeLevel);
        rotationDirection = hailSlimeData.rotationDirection;
        isDragging = false;
        travel = 0f;
        base.InitSlime(); // 강화 능력이 값을 바꿀 수 있으므로 기본값 설정 후 호출

        // 개수만큼 눈 준비 (남는 건 끔)
        while (hails.Count < hailCount)
            hails.Add(Instantiate(hailPrefab, transform));
        for (int i = 0; i < hails.Count; i++)
        {
            hails[i].Init(this, hailSlimeData.hitCooldown);
            hails[i].iceChance = iceChance;
            hails[i].iceDuration = hailSlimeData.iceDuration;
            hails[i].gameObject.SetActive(false); // 경로 위치를 정한 뒤 UpdateHails에서 켬
        }
    }

    public float MoveSpeed => hailSlimeData.moveSpeed * moveSpeedMul * AttackSpeed / GameSetting.ATTACK_SPEED_PER_SECOND;

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
