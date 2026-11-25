// [B] 산들바람: 기본 탄 발사 + 십자 4칸 슬라임에게 [공격속도 증가] 버프 (공통 처리는 CrossBuffMinion)
public class BreezeMinion : CrossBuffMinion
{
    protected override StatType BuffStatType => StatType.AttackSpeed;
}
