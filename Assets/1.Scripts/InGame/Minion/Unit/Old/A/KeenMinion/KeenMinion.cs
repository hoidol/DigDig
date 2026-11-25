// [A] 설의: 기본 탄 발사 + 십자 4칸 슬라임에게 [크리티컬 확률 증가] 버프 (공통 처리는 CrossBuffMinion)
// 슬라임의 CritChance 스탯은 탄 발사 시 AllyUnitDamageData.critChance로 들어감
public class KeenMinion : CrossBuffMinion
{
    protected override StatType BuffStatType => StatType.CritChance;
}
