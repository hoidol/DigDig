// 성장 시간 단축 (5: 25초 > 22초, 11: 22초 > 18초). 둘 다 켜지면 짧은 쪽
public class GrowUpTimeEnhanceAbility : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    public float growTime = 20f;
    public void Force(Slime slime)
    {
        GrowUpSlime growUp = slime as GrowUpSlime;
        if (growTime < growUp.growTime)
            growUp.growTime = growTime;
    }
}
