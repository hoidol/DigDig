// (2026-11-20) 미니언 소환 수: 필드에 둘 수 있는 최대 미니언 수(MinionFieldController.minionMaxCount) +countPerLevel / 레벨
// 룬 "슬라임 수 증가"(AddMinionSpawnCountItem)와 같은 값에 더함
public class MaxMinionCountKingAbility : KingAbility
{
    public int countPerLevel = 1;

    public override string GetValueText(int level) => (countPerLevel * level).ToString();

    public override void OnEquip() => Add(countPerLevel);
    public override void OnLevelChanged() => Add(countPerLevel);
    public override void OnUnequip() => Add(-countPerLevel * level);

    static void Add(int count)
    {
        if (MinionFieldController.Instance != null)
            MinionFieldController.Instance.minionMaxCount += count;
    }
}
