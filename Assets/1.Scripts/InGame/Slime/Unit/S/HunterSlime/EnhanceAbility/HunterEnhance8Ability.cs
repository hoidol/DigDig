// [각성] 2회 공격마다 저격탄 발사 (기본 3회)
public class HunterEnhance8Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    public int snipeInterval = 2;
    public void Force(Slime slime)
    {
        (slime as HunterSlime).snipeInterval = snipeInterval;
    }
}
