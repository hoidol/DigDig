public class HailEnhance11Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 눈 이동 속도 15% 증가
    public float moveSpeedMul = 1.15f;
    public void Force(Slime slime)
    {
        (slime as HailSlime).moveSpeedMul *= moveSpeedMul;
    }
}
