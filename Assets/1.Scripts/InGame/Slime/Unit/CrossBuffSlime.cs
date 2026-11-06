using System.Collections.Generic;
using UnityEngine;

// 십자 4칸 버프 슬라임 공용 (Breeze 공속 / Rage 공격력 / Keen 치명타 확률)
// 기본 탄 발사 + 내 타일 기준 상하좌우 4칸 슬라임에게 Buff(BuffStatType, 값, BuffOpType)
// 범위는 내 타일의 TileGroup 안에서만 (바로 옆 칸이라도 그룹이 다르면 영향 없음)
// 타일 위 오브젝트가 바뀔 때마다(소환/이동/머지) 대상을 다시 계산 → 들어온 슬라임은 버프, 나간 슬라임은 해제
// 여러 버프 슬라임이 같은 슬라임을 감싸면 버프는 각각 적용 (Multiply는 곱, Add는 합)
public abstract class CrossBuffSlime : SlimeGrowth1
{
    static readonly Vector2Int[] CROSS_OFFSETS =
    {
        new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0),
    };
    static readonly Vector2Int[] EIGHT_OFFSETS =
    {
        new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0),
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1),
    };

    protected CrossBuffSlimeData crossBuffSlimeData;
    AllyBulletSpec allyBulletSpec;
    // 강화 능력이 바꿀 수 있음 (InitSlime에서 base 호출 전에 기본값 설정)
    public float buffValue;
    public List<Buff> extraBuffs = new List<Buff>();       // 버프 대상에게 같이 거는 추가 버프 (강화 11 등)
    // [각성] 포위 진형: 십자 4칸이 모두 슬라임이면 버프 효과(증가분) × (1 + surroundBonus). 0이면 없음
    // 같은 TileGroup 안 십자 4칸이 다 있어야 하므로 사실상 3*3 그룹의 가운데 칸 + 막힌 칸 없음이 조건
    public float surroundBonus;

    // 버프 범위 확장 타일(ExpandBuffRangeTileItem) 위에 있으면 십자 4칸 → 8방향. 타일이 OnEnter/OnExit로 올리고 내림
    // InitSlime에서 초기화하지 않음 (타일 상태라 강화 재적용과 무관)
    int rangeExpandCount;
    public bool IsRangeExpanded => rangeExpandCount > 0;
    public void AddRangeExpand() { rangeExpandCount++; RefreshBuffs(); }
    public void RemoveRangeExpand() { rangeExpandCount = Mathf.Max(0, rangeExpandCount - 1); RefreshBuffs(); }

    protected abstract StatType BuffStatType { get; }
    protected virtual StatOpType BuffOpType => crossBuffSlimeData.buffOpType;

    // 버프를 준 슬라임 → 걸어준 Buff들 (RemoveBuff는 참조로 지우므로 인스턴스를 들고 있음)
    readonly Dictionary<Slime, List<Buff>> appliedBuffs = new Dictionary<Slime, List<Buff>>();
    float appliedValue; // 지금 걸려 있는 버프 값 (포위 여부로 바뀌면 전부 다시 걸기)

    // 포위 진형 보너스까지 반영한 실제 버프 값. Multiply는 증가분에만 배율 (1.2 → 1.3), Add는 값 전체 (20 → 30)
    public float EffectiveBuffValue
    {
        get
        {
            if (surroundBonus <= 0 || !IsSurrounded())
                return buffValue;
            float mul = 1f + surroundBonus;
            return BuffOpType == StatOpType.Multiply ? 1f + (buffValue - 1f) * mul : buffValue * mul;
        }
    }

    // 같은 TileGroup 안 십자 4칸이 모두 있고(그룹 경계/막힌 칸이면 실패) 모두 슬라임이 올라가 있는지
    public bool IsSurrounded()
    {
        if (TileManager.Instance == null)
            return false;
        Tile center = TileManager.Instance.GetTile(this);
        if (center == null)
            return false;

        int count = 0;
        foreach (Tile tile in TileManager.Instance.GetTilesAround(center, CROSS_OFFSETS))
        {
            if (tile.tileGroup != center.tileGroup || tile.tileObject is not Slime)
                return false;
            count++;
        }
        return count == CROSS_OFFSETS.Length;
    }

    public override void Spawn(Vector2 pos, int lv)
    {
        crossBuffSlimeData = SlimeManager.Instance.GetSlimeData(key) as CrossBuffSlimeData;
        base.Spawn(pos, lv);
    }

    public override void InitSlime()
    {
        allyBulletSpec = new AllyBulletSpec();
        buffValue = crossBuffSlimeData.GetBuffValue(mergeLevel);
        extraBuffs.Clear();
        surroundBonus = 0;
        base.InitSlime(); // 강화 능력이 buffValue를 바꿀 수 있으므로 기본값 설정 후 호출

        // 풀링 재사용 등으로 값이 바뀌었으면 새 값으로 다시 걸기
        ClearBuffs();
        RefreshBuffs();
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;
        allyBulletSpec.StartBulletSpec();
        return allyBulletSpec.Instantiate(this);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        GameEventBus.Subscribe<TileObjectChangedEvent>(OnTileObjectChanged);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        GameEventBus.Unsubscribe<TileObjectChangedEvent>(OnTileObjectChanged);
        ClearBuffs();
    }

    void OnTileObjectChanged(TileObjectChangedEvent e)
    {
        RefreshBuffs();
    }

    // 버프 대상 조건. 하위 클래스에서 제외 규칙을 추가할 수 있음
    protected virtual bool CanBuff(Slime slime) => slime != this;

    // 내 타일 기준 십자 4칸(버프 범위 확장 타일 위면 8방향), 같은 TileGroup 안의 슬라임 (내 타일이 없으면 빈 목록)
    List<Slime> GetTargets()
    {
        List<Slime> targets = new List<Slime>();
        if (TileManager.Instance == null)
            return targets;

        Tile center = TileManager.Instance.GetTile(this);
        if (center == null)
            return targets;

        foreach (Tile tile in TileManager.Instance.GetTilesAround(center, IsRangeExpanded ? EIGHT_OFFSETS : CROSS_OFFSETS))
        {
            if (tile.tileGroup != center.tileGroup)
                continue;
            if (tile.tileObject is Slime slime && CanBuff(slime))
                targets.Add(slime);
        }
        return targets;
    }

    void RefreshBuffs()
    {
        if (crossBuffSlimeData == null) // Spawn 전
            return;

        List<Slime> targets = GetTargets();

        // 포위 진형 등으로 버프 값이 바뀌었으면 전부 해제 후 새 값으로 다시 걸기
        float value = EffectiveBuffValue;
        if (!Mathf.Approximately(value, appliedValue))
        {
            ClearBuffs();
            appliedValue = value;
        }

        // 범위에서 빠진 슬라임 버프 해제
        List<Slime> removed = new List<Slime>();
        foreach (var pair in appliedBuffs)
        {
            if (!targets.Contains(pair.Key))
                removed.Add(pair.Key);
        }
        foreach (Slime slime in removed)
        {
            if (slime != null) // 이미 Destroy된 슬라임은 해제 생략
                RemoveBuffs(slime, appliedBuffs[slime]);
            appliedBuffs.Remove(slime);
        }

        // 새로 들어온 슬라임 버프 적용
        foreach (Slime slime in targets)
        {
            if (appliedBuffs.ContainsKey(slime))
                continue;
            // 슬라임마다 따로 인스턴스 (extraBuffs는 템플릿)
            List<Buff> buffs = new List<Buff> { new Buff(BuffStatType, appliedValue, BuffOpType) };
            foreach (Buff extra in extraBuffs)
                buffs.Add(new Buff(extra.statType, extra.value, extra.opType));
            appliedBuffs.Add(slime, buffs);
            foreach (Buff buff in buffs)
                slime.AddBuff(buff);
        }
    }

    static void RemoveBuffs(Slime slime, List<Buff> buffs)
    {
        foreach (Buff buff in buffs)
            slime.RemoveBuff(buff);
    }

    void ClearBuffs()
    {
        foreach (var pair in appliedBuffs)
        {
            if (pair.Key != null)
                RemoveBuffs(pair.Key, pair.Value);
        }
        appliedBuffs.Clear();
    }
}
