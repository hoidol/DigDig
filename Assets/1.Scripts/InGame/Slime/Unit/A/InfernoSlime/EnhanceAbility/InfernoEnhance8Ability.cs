public class InfernoEnhance8Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // [각성] 지옥불 피해를 입은 적에게 화상 (InfernoSlimeData.burnDuration / burnDpsRatio)
    public void Force(Slime slime)
    {
        (slime as InfernoSlime).applyBurn = true;
    }
}
