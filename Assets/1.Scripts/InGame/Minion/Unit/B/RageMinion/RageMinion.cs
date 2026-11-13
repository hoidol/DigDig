// [B] 격노: 기본 탄 발사 + 십자 4칸 슬라임에게 [공격력 증가] 버프 (공통 처리는 CrossBuffMinion)
public class RageMinion : CrossBuffMinion
{
    protected override StatType BuffStatType => StatType.AttackPower;
}
