// (2026-11-24) 용사 강화 단계(Lv.3·4·6·7)의 에너지 카드. 효과는 HeroKingAbility가 읽어 씀 (이 능력은 값만 들고 있음)
// - ChargeTime = (2026-11-25) 충전 시간 -N% (30초 기준, 최소 GameSetting.HERO_MIN_CHARGE_TIME). 이전: -N초
// - MaxCharges = (2026-11-25) 에너지 최대 보유 +N개 (기본 GameSetting.HERO_ENERGY_BASE_MAX = 2)
// - EliteKill·KingKill = (2026-11-25 폐기, 효과 없음. 처치로 충전 감소 삭제) enum만 남김
// 용사 강화 단계 카드 = 정예 칸 추가 · 충전 가속 · 에너지 보유 증가 · 스킬 강화 (LevelUpChoiceRule.PickHeroBoostChoices)
public class HeroEnergyKingAbility : KingAbility
{
    public HeroEnergyBoostType boostType = HeroEnergyBoostType.ChargeTime;
    public float valuePerLevel = 8f;

    public float GetValue(int level) => valuePerLevel * level;

    public override string GetValueText(int level) => FormatNumber(GetValue(level));
}

// 에셋에 번호로 → 뒤에만 추가
public enum HeroEnergyBoostType
{
    ChargeTime,
    EliteKill,
    KingKill,
    MaxCharges,
}
