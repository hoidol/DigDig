// 골드 획득 범위 15% 증가
public class ScavengerEnhance5Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    public float rangeMul = 1.15f;
    public void Force(Slime slime)
    {
        (slime as ScavengerSlime).coinRangeMul *= rangeMul;
    }
}
