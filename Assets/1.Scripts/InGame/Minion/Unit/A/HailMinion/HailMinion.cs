using System.Collections.Generic;
using UnityEngine;

// [A] 빙석 (2026-11-15 스킬형으로 변경)
// - 평소엔 기본 탄 공격
// - 스킬: SKILL_COOLTIME초마다 눈(HailStone) hailCount개를 소환 → SKILL_DURATION초 동안 내 TileGroup의 적 경로(enemyRoots)를 순환한 뒤 사라짐
//   눈이 남아 있어도 쿨타임은 계속 참 → 겹쳐서 여러 개가 돌 수 있음. 경로가 없는 그룹이면 찬 채로 대기
// - 경로는 enemyRoots[0] → [1] → … → 마지막 → 다시 [0] (닫힌 고리). 한 번에 여러 개면 같은 간격으로
// - 이동 속도 = moveSpeed × 공속 / ATTACK_SPEED_PER_SECOND (공속 버프가 속도에 반영)
// - 닿은 적: 피해(공격력 × stoneDamageMul, 머지 레벨별 150%·170%…) + iceChance%(머지 레벨별) 확률로 얼음
// - 슬라임을 다른 그룹으로 옮기면 그 그룹 경로로 바뀜. 드래그 중에는 눈 숨김
// - [각성] (진화) 한 번에 눈 EVOLVE_HAIL_COUNT개
// 프리팹: hailPrefab에 HailStone 프리팹 연결
public class HailMinion : MinionGrowth1, IRotationDirectionUnit
{
    // 경로를 도는 방향 설정 (설정 버튼)
    public RotationDirection RotationDirection => rotationDirection;
    public void SetRotationDirection(RotationDirection direction) => rotationDirection = direction;

    public const int EVOLVE_HAIL_COUNT = 2;
    public const float SKILL_COOLTIME = 10f;
    public const float SKILL_DURATION = 12f;

    public HailStone hailPrefab;

    HailMinionData hailMinionData;
    AllyBulletSpec allyBulletSpec;

    public int hailCount;
    public float iceChance;
    public float stoneDamageMul; // 눈 피해 배율 (머지 레벨별)
    public RotationDirection rotationDirection; // 기본값은 데이터, 설정 버튼으로 변경

    // 소환된 눈 하나: 경로 위 진행 거리 + 남은 시간
    class ActiveHail
    {
        public HailStone stone;
        public float travel;
        public float remain;
    }

    readonly List<HailStone> stonePool = new List<HailStone>();
    readonly List<ActiveHail> activeHails = new List<ActiveHail>();
    float skillTimer;
    bool isDragging;

    public override float SkillProgress => Mathf.Clamp01(skillTimer / SKILL_COOLTIME);
    public override void FillSkillForTest() { skillTimer = SKILL_COOLTIME; }

    public override void Spawn(Vector2 pos, int lv)
    {
        hailMinionData = MinionManager.Instance.GetMinionData(key) as HailMinionData;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        allyBulletSpec = new AllyBulletSpec();
        hailCount = hailMinionData.hailCount;
        iceChance = hailMinionData.iceChances.GetByMergeLevel(mergeLevel);
        stoneDamageMul = hailMinionData.stoneDamageMuls.GetByMergeLevel(mergeLevel);
        rotationDirection = hailMinionData.rotationDirection;
        isDragging = false;
        skillTimer = 0f;
        ClearHails();
        base.InitMinion();
    }

    protected override void OnEvolve()
    {
        hailCount = Mathf.Max(hailCount, EVOLVE_HAIL_COUNT);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        ClearHails();
    }

    public float MoveSpeed => hailMinionData.moveSpeed * AttackSpeed / GameSetting.ATTACK_SPEED_PER_SECOND;

    public override void Update()
    {
        base.Update();
        UpdateSkill();
        UpdateHails();
    }

    void UpdateSkill()
    {
        if (skillTimer < SKILL_COOLTIME)
        {
            skillTimer += SkillDeltaTime;
            return;
        }
        Transform[] path = GetPath();
        float total = PathLoop.GetLength(path);
        if (isDragging || total <= 0f)
            return; // 경로가 없으면 찬 채로 대기
        skillTimer = 0f;
        SummonHails(path, total);
        PlaySkillAnim();
    }

    // hailCount개를 경로 위 같은 간격으로 소환 (첫 눈은 슬라임과 가장 가까운 경로 지점부터)
    void SummonHails(Transform[] path, float total)
    {
        float start = PathLoop.GetNearestTravel(path, total, transform.position);
        float gap = total / Mathf.Max(1, hailCount);
        for (int i = 0; i < hailCount; i++)
        {
            HailStone stone = GetStone();
            activeHails.Add(new ActiveHail { stone = stone, travel = start + gap * i, remain = SKILL_DURATION });
        }
    }

    HailStone GetStone()
    {
        HailStone stone = stonePool.Find(s => !s.gameObject.activeSelf && !activeHails.Exists(a => a.stone == s));
        if (stone == null)
        {
            stone = Instantiate(hailPrefab, transform);
            stonePool.Add(stone);
        }
        stone.Init(this, hailMinionData.hitCooldown);
        stone.iceChance = iceChance;
        stone.iceDuration = hailMinionData.iceDuration;
        stone.gameObject.SetActive(false); // 위치를 정한 뒤 UpdateHails에서 켬
        return stone;
    }

    void ClearHails()
    {
        foreach (ActiveHail h in activeHails)
            if (h.stone != null)
                h.stone.gameObject.SetActive(false);
        activeHails.Clear();
    }

    void UpdateHails()
    {
        if (activeHails.Count == 0)
            return;

        Transform[] path = GetPath();
        float total = PathLoop.GetLength(path);
        bool show = !isDragging && total > 0f;
        bool playing = GameManager.Instance == null || GameManager.Instance.isPlaying;
        float step = PathLoop.GetTravelSign(path, rotationDirection) * MoveSpeed * Time.deltaTime;
        float damage = AttackPower * stoneDamageMul;

        for (int i = activeHails.Count - 1; i >= 0; i--)
        {
            ActiveHail h = activeHails[i];
            h.remain -= Time.deltaTime;
            if (h.remain <= 0f || h.stone == null)
            {
                if (h.stone != null)
                    h.stone.gameObject.SetActive(false);
                activeHails.RemoveAt(i);
                continue;
            }

            if (h.stone.gameObject.activeSelf != show)
                h.stone.gameObject.SetActive(show);
            if (!show)
                continue;

            if (playing)
            {
                h.travel = Mathf.Repeat(h.travel + step, total);
                h.stone.Spin(rotationDirection, Time.deltaTime);
            }
            h.stone.damage = damage;
            h.stone.transform.position = PathLoop.GetPoint(path, total, h.travel);
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

    // 평소 기본 탄
    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;
        allyBulletSpec.StartBulletSpec();
        return allyBulletSpec.Instantiate(this);
    }
}
