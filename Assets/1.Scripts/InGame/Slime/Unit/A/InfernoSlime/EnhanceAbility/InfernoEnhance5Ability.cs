public class InfernoEnhance5Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 화염(지옥불) 피해 15% 증가
    public float damageMul = 1.15f;
    public void Force(Slime slime)
    {
        (slime as InfernoSlime).damageMul *= damageMul;
    }
}
