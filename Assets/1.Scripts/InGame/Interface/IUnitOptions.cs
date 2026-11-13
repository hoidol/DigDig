// 선택한 유닛 설정 UI(AllyOptionButton)에서 바꿀 수 있는 슬라임별 설정

// 버프 범위 모양을 고를 수 있는 유닛 (CrossBuffMinion)
public interface IBuffRangeUnit
{
    BuffRangeShape BuffRangeShape { get; }
    void SetBuffRangeShape(BuffRangeShape shape);
}

public enum BuffRangeShape
{
    Cross,     // 십자 4칸
    Diagonal,  // 대각 4칸
}

// 회전 방향을 고를 수 있는 유닛 (회전체: Orbit/Mud/Shield, 경로 순환: Hail)
public interface IRotationDirectionUnit
{
    RotationDirection RotationDirection { get; }
    void SetRotationDirection(RotationDirection direction);
}
