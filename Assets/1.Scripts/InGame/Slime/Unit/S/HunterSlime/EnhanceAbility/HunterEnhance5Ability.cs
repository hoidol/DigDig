// 저격탄 스턴 0.2초 > 0.3초
public class HunterEnhance5Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    public float stunDuration = 0.3f;
    public void Force(Slime slime)
    {
        (slime as HunterSlime).snipeStunDuration = stunDuration;
    }
}
