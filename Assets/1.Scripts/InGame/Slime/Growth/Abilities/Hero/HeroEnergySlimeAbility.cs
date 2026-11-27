// (2026-11-24) 용사 강화 단계의 에너지 카드. 효과는 HeroSlimeAbility가 읽어 씀 (이 능력은 값만 들고 있음)
// - ChargeTime = (2026-11-26) 충전 시간 ×(1 - N%)^레벨 (곱하기: 20%면 30 → 24 → 19.2 → 15.4 → 12.3 → 9.8초, 최소 GameSetting.HERO_MIN_CHARGE_TIME)
//   (이전 2026-11-25: -N%/Lv 더하기 → 5레벨이면 100%가 돼서 바꿈)
// - MaxCharges = (2026-11-25) 에너지 최대 보유 +N개 (기본 GameSetting.HERO_ENERGY_BASE_MAX = 2)
// - EliteKill·SlimeKill = (2026-11-25 폐기, 효과 없음. 처치로 충전 감소 삭제) enum만 남김
// 용사 강화 단계 카드 = 정예 칸 추가 · 충전 가속 · 에너지 보유 증가 · 스킬 강화 (LevelUpChoiceRule.PickHeroBoostChoices)
public class HeroEnergySlimeAbility : SlimeAbility
{
    public HeroEnergyBoostType boostType = HeroEnergyBoostType.ChargeTime;
    public float valuePerLevel = 8f;

    public float GetValue(int level) => valuePerLevel * level;

    // 충전 시간 배율 (ChargeTime 카드): (1 - N%)^레벨
    public float GetChargeTimeMul(int level) => UnityEngine.Mathf.Pow(UnityEngine.Mathf.Clamp01(1f - valuePerLevel / 100f), level);

    // 카드 설명 {0}: 충전 가속 = "-20% (30초 → 24초)" (level = 고르면 될 레벨), 그 외 = 값
    public override string GetValueText(int level)
    {
        if (boostType != HeroEnergyBoostType.ChargeTime)
            return FormatNumber(GetValue(level));
        float baseTime = GameSetting.HERO_ENERGY_CHARGE_TIME;
        float from = UnityEngine.Mathf.Max(GameSetting.HERO_MIN_CHARGE_TIME, baseTime * GetChargeTimeMul(level - 1));
        float to = UnityEngine.Mathf.Max(GameSetting.HERO_MIN_CHARGE_TIME, baseTime * GetChargeTimeMul(level));
        return $"-{FormatNumber(valuePerLevel)}% ({FormatNumber(UnityEngine.Mathf.Round(from * 10f) / 10f)}초 → {FormatNumber(UnityEngine.Mathf.Round(to * 10f) / 10f)}초)";
    }
}

// 에셋에 번호로 → 뒤에만 추가
public enum HeroEnergyBoostType
{
    ChargeTime,
    EliteKill,
    SlimeKill,
    MaxCharges,
}
