using System.Collections.Generic;
using UnityEngine;

// (2026-11-21) Lv.15 패시브 공용 부모. 종족별 풀 (raceType = 그 종족일 때만 제시), 고른 뒤 능력치 카드 사이에 Lv.2·3 강화 카드로 섞여 나옴
// - 수치는 레벨별 배열(index = 레벨 - 1)로 프리팹 인스펙터에서 수정 → LV(배열)
// - 공용 도우미: 버프 칸(Area), 적 처치 이벤트(OnEnemyKilled), 킹 사거리 판정(InKingRange)
// - 하위 클래스: 효과 적용은 Apply()(장착·레벨업마다 다시 호출), 해제는 Clear()
public abstract class PassiveKingAbility : KingAbility
{
    public KingRaceType raceType;
    public PassiveKind kind;

    KingTileBuffArea area;
    protected KingTileBuffArea Area => area != null ? area : (area = KingTileBuffArea.GetOrCreate(king));

    // 레벨별 값 (배열이 짧으면 마지막 값)
    protected float LV(float[] values) => LV(values, level);
    protected static float LV(float[] values, int level)
    {
        if (values == null || values.Length == 0)
            return 0f;
        return values[Mathf.Clamp(level - 1, 0, values.Length - 1)];
    }
    protected static int LV(int[] values, int level)
    {
        if (values == null || values.Length == 0)
            return 0;
        return values[Mathf.Clamp(level - 1, 0, values.Length - 1)];
    }
    protected int LV(int[] values) => LV(values, level);

    // 설명 표시용: 0.15 → "15"
    protected static string Percent(float ratio) => FormatNumber(ratio * 100f);

    // 적이 처치될 때마다 OnEnemyKilled (경로 끝 도착 제외). 하위 클래스가 쓰면 ListenKills = true
    protected virtual bool ListenKills => false;

    public override void OnEquip()
    {
        if (ListenKills)
            GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
        Apply();
    }

    public override void OnLevelChanged() => Apply();

    public override void OnUnequip()
    {
        if (ListenKills)
            GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
        Clear();
    }

    protected virtual void OnDestroy()
    {
        if (ListenKills)
            GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
    }

    protected abstract void Apply();
    protected virtual void Clear() { }

    void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        if (e.enemy == null || e.enemy.CurHp > 0) // 경로 끝 도착은 제외
            return;
        OnEnemyKilled(e.enemy, e.position);
    }

    protected virtual void OnEnemyKilled(Enemy enemy, Vector2 pos) { }

    protected bool InKingRange(Vector2 pos)
    {
        if (king == null || king.statMgr == null)
            return false;
        return Vector2.Distance(king.transform.position, pos) <= king.statMgr.AttackRange;
    }

    protected List<Minion> AreaMinions => Area.GetTargetMinions();
}

// 패시브 종류 (카드 표시용, 에셋에 번호로 → 뒤에만 추가)
public enum PassiveKind
{
    Buff,   // 버프형: 버프 칸·배치
    Attack, // 공격형: 킹이 적을 직접 다룸
}
