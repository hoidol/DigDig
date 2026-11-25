// (2026-11-25) 용사 강화 카드 "1단계 스킬 강화" / "2단계 스킬 강화" (최대 1). 그 스킬을 해방한 뒤부터 나옴 (1단계 = Lv.2, 2단계 = Lv.5 해방 이후)
// 효과는 용사마다 (HeroKingAbility.IsSkillEnhanced → 각 용사 클래스가 수치에 반영, GetSkillEnhanceText = 카드 설명). 이 능력은 단계 번호만 들고 있음
public class HeroSkillEnhanceKingAbility : KingAbility
{
    public int skillIndex; // 0 = 1단계, 1 = 2단계

    // 카드 설명 {0} = 지금 용사의 "스킬 이름: 강화 내용"
    public override string GetValueText(int level)
    {
        HeroKingAbility hero = KingSlime.Instance != null && KingSlime.Instance.abilityInventory != null
            ? KingSlime.Instance.abilityInventory.Get<HeroKingAbility>() : null;
        return hero != null ? $"<b>{hero.GetSkillName(skillIndex)}</b>\n{hero.GetSkillEnhanceText(skillIndex)}" : "";
    }
}
