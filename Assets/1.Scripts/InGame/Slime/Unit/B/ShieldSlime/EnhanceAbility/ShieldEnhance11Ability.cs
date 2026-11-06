public class ShieldEnhance11Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 방패 회전 속도 15% 증가
    public float rotationSpeedMul = 1.15f;
    public void Force(Slime slime)
    {
        (slime as ShieldSlime).rotationSpeed *= rotationSpeedMul;
    }
}
