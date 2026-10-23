// 발사 방식을 바꿀 수 있는 유닛 (Slime, Character). 타일 아이템이 OnEnter/OnExit에서 값을 더하고 뺀다
public interface IShotModifiable
{
    ShotModifier ShotModifier { get; }
}

[System.Serializable]
public class ShotModifier
{
    public int bonusShootCount;    // 다중 타겟: 한 번 공격에 추가로 쏘는 탄 수 (서로 다른 타겟)
    public int randomShotInterval; // N번째 공격마다 랜덤 타겟 (0이면 꺼짐)
    public TileItem tileItem;      // 올라가 있는 타일의 아이템 (발사 시 OnFired 호출). Tile이 연결/해제

    int attackCount;

    public bool IsRandomShotTurn => randomShotInterval > 0 && (attackCount + 1) % randomShotInterval == 0;

    // 실제로 발사한 공격만 센다
    public void CountAttack() => attackCount++;
}
