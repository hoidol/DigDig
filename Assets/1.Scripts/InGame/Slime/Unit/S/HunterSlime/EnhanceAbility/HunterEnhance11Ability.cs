// 저격탄 공격력 30% 증가
public class HunterEnhance11Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    public float snipeDamageMul = 1.3f;
    public void Force(Slime slime)
    {
        (slime as HunterSlime).snipeDamageMul *= snipeDamageMul;
    }
}
