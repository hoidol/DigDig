// [각성] 엘리트, 보스 처치 시 30골드 추가
public class ScavengerEnhance8Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    public int bonusCoin = 30;
    public void Force(Slime slime)
    {
        (slime as ScavengerSlime).eliteBossBonusCoin += bonusCoin;
    }
}
