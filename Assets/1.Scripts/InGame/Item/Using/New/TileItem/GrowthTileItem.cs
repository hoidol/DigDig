using UnityEngine;

// (2026-11-21) 성장 룬: wave가 끝날 때마다(브레이크타임 시작) 이 칸 공격력 +perWave% 누적, 최대 = 레벨 값%(Values 10/15/20)
// 누적은 룬(칸)에 쌓임 → 위 미니언이 합성으로 바뀌어도 유지. 일찍 새길수록 이득
public class GrowthTileItem : TileItem
{
    public float perWave = 2f;  // %
    public float maxBonus = 10f; // % (레벨 값)
    public float bonus;          // 지금 누적 %

    IBuffable target;
    Buff appliedBuff;

    protected override void ApplyLevelValue(float v) => maxBonus = v;
    protected override string FormatLevelValue(float v) => FormatNumber(v);

    public override string GetDescription() => base.GetDescription() + $"\n(지금 +{FormatNumber(bonus)}%)";

    void OnEnable() => GameEventBus.Subscribe<BreakTimeStartEvent>(OnBreakTime);
    void OnDisable() => GameEventBus.Unsubscribe<BreakTimeStartEvent>(OnBreakTime);

    void OnBreakTime(BreakTimeStartEvent e)
    {
        if (tile == null || bonus >= maxBonus)
            return;
        bonus = Mathf.Min(maxBonus, bonus + perWave);
        Reapply();
    }

    public override void OnEnter(ITileObject tileObject)
    {
        if (tileObject is not IBuffable t)
            return;
        target = t;
        Reapply();
    }

    public override void OnExit(ITileObject tileObject)
    {
        if (target == null || !ReferenceEquals(target, tileObject))
            return;
        if (appliedBuff != null)
            target.RemoveBuff(appliedBuff);
        appliedBuff = null;
        target = null;
    }

    void Reapply()
    {
        if (target == null || (target is Object o && o == null))
            return;
        if (appliedBuff != null)
            target.RemoveBuff(appliedBuff);
        appliedBuff = null;
        if (bonus <= 0f)
            return;
        appliedBuff = new Buff(StatType.AttackPower, 1f + bonus / 100f, StatOpType.Multiply);
        target.AddBuff(appliedBuff);
    }
}
