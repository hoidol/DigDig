public class DopamineEnhance11Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 추가 발사 쿨타임 20% 감소
    public float coolTimeMul = 0.8f;
    public void Force(Slime slime)
    {
        (slime as DopamineSlime).coolTime *= coolTimeMul;
    }
}
