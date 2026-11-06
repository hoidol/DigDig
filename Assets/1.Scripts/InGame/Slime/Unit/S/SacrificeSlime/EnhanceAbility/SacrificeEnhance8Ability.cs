// [각성] 합성 시 30% 확률로 머지 레벨 +1 추가
public class SacrificeEnhance8Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    public float extraLevelChance = 0.3f;
    public void Force(Slime slime)
    {
        (slime as SacrificeSlime).extraLevelChance = extraLevelChance;
    }
}
