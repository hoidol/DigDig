// (2026-11-21) 스킬 쿨타임 룬: 이 칸 미니언 스킬 쿨타임 충전 속도 +레벨 값% (Values 20/40/60, StatType.SkillCooldownSpeed → Minion.SkillDeltaTime)
// 쿨타임 스킬이 없는 미니언은 효과 없음
public class SkillCooldownTileItem : StatBuffTileItem
{
    protected override StatType StatType => StatType.SkillCooldownSpeed;

    void Reset()
    {
        opType = StatOpType.Add;
        value = 20f;
    }
}
