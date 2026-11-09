// 회전체 슬라임(Orbit/Mud/Shield) · 경로 순환 슬라임(Hail) 회전 방향: 시계 ↔ 반시계
public class SetRotationDirectionButton : AllyOptionButton
{
    public override bool IsAvailable(IAllyUnit unit) => unit is IRotationDirectionUnit;

    protected override void SetNext()
    {
        IRotationDirectionUnit unit = (IRotationDirectionUnit)targetAlly;
        unit.SetRotationDirection(unit.RotationDirection == RotationDirection.Clockwise
            ? RotationDirection.CounterClockwise
            : RotationDirection.Clockwise);
    }

    protected override string GetText()
    {
        return TranslateManager.GetText($"Rotation.{((IRotationDirectionUnit)targetAlly).RotationDirection}");
    }
}
