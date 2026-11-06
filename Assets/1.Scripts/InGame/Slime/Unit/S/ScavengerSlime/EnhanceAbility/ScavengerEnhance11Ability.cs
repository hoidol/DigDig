// 20% 확률로 골드 2배
public class ScavengerEnhance11Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    public float doubleChance = 0.2f;
    public void Force(Slime slime)
    {
        (slime as ScavengerSlime).doubleChance = doubleChance;
    }
}
