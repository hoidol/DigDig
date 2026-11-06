public class BeamEnhance11Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 연결될수록 줄어드는 피해 감소율 완화 (레이저 1개당 감소율 × reductionMul)
    public float reductionMul = 0.5f;
    public void Force(Slime slime)
    {
        (slime as BeamSlime).damageReductionPerLine *= reductionMul;
    }
}
