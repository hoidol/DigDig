public class DopamineEnhance5Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 적 처치 감지 범위 15% 증가
    public float rangeMul = 1.15f;
    public void Force(Slime slime)
    {
        (slime as DopamineSlime).detectRange *= rangeMul;
    }
}
