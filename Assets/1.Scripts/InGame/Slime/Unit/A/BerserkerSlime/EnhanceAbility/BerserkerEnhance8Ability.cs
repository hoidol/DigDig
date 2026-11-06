public class BerserkerEnhance8Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // [각성] 최대 중첩 시 공격력 10% 증가 (BerserkerSlime.UpdateSlime에서 적용)
    public float attackPowerMul = 1.1f;
    public void Force(Slime slime)
    {
        (slime as BerserkerSlime).maxStackAttackPowerMul = attackPowerMul;
    }
}
