// 십자 버프 슬라임(CrossBuffMinion) 버프 범위 모양: 십자 4칸 ↔ 대각 4칸
public class SetBuffRangeButton : AllyOptionButton
{
    public override bool IsAvailable(IAllyUnit unit) => unit is IBuffRangeUnit;

    protected override void SetNext()
    {
        IBuffRangeUnit unit = (IBuffRangeUnit)targetAlly;
        unit.SetBuffRangeShape(unit.BuffRangeShape == BuffRangeShape.Cross ? BuffRangeShape.Diagonal : BuffRangeShape.Cross);
    }

    protected override string GetText()
    {
        return TranslateManager.GetText($"BuffRange.{((IBuffRangeUnit)targetAlly).BuffRangeShape}");
    }
}
