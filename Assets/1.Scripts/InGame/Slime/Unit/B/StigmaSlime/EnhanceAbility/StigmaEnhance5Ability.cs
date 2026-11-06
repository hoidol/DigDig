public class StigmaEnhance5Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 낙인이 적은 적을 우선 공격 (StigmaSlime.FindTarget(s))
    public void Force(Slime slime)
    {
        (slime as StigmaSlime).targetLowStigmaFirst = true;
    }
}
