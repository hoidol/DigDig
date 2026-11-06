public class MudEnhance5Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 진흙 지속 시간 +0.5초
    public float addMudDuration = 0.5f;
    public void Force(Slime slime)
    {
        (slime as MudSlime).mudDuration += addMudDuration;
    }
}
