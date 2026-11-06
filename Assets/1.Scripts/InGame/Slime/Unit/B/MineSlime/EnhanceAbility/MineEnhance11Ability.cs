public class MineEnhance11Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 지뢰 최대 설치 수 +2
    public int addMaxMineCount = 2;
    public void Force(Slime slime)
    {
        (slime as MineSlime).maxMineCount += addMaxMineCount;
    }
}
