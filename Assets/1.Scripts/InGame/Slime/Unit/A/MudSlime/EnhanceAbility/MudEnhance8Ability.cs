public class MudEnhance8Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 진흙 구슬 최대 수 +1
    public int addOrbCount = 1;
    public void Force(Slime slime)
    {
        (slime as MudSlime).maxOrbCount += addOrbCount;
    }
}
