// [각성] 성장 시 50% 확률로 머지 레벨 +1 추가
public class GrowUpEnhance8Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    public float extraLevelChance = 0.5f;
    public void Force(Slime slime)
    {
        (slime as GrowUpSlime).extraLevelChance = extraLevelChance;
    }
}
