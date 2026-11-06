// 십자 버프 슬라임 공용: 버프 값 증가 (Multiply 0.05 = +5%p, Add면 그대로 더함)
// ex) Breeze/Rage 5: 버프 증가량 +5%p
public class CrossBuffValueEnhanceAbility : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    public float addValue = 0.05f;
    public void Force(Slime slime)
    {
        (slime as CrossBuffSlime).buffValue += addValue;
    }
}
