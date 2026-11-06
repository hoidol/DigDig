public class InfernoEnhance11Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 화상 지속 시간 +1초
    public float addBurnDuration = 1f;
    public void Force(Slime slime)
    {
        (slime as InfernoSlime).burnDuration += addBurnDuration;
    }
}
