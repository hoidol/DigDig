public class MineEnhance5Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 지뢰 폭발 범위 15% 증가
    public float radiusMul = 1.15f;
    public void Force(Slime slime)
    {
        (slime as MineSlime).explodeRadius *= radiusMul;
    }
}
