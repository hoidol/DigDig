public class MudEnhance11Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 회전 반경 15% 증가
    public float radiusMul = 1.15f;
    public void Force(Slime slime)
    {
        (slime as MudSlime).radius *= radiusMul;
    }
}
