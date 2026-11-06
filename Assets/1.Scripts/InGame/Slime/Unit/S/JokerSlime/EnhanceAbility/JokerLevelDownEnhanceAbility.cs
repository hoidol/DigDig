// 변신 실패 확률 감소: 기본 30% → 5강 25% → 8강 20% → 11강 15%
// 프리팹에 레벨별로 하나씩 붙이고 levelDownChance 입력. 여러 개 켜지면 가장 낮은 값
public class JokerLevelDownEnhanceAbility : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    public float levelDownChance = 0.25f;
    public void Force(Slime slime)
    {
        JokerSlime joker = slime as JokerSlime;
        if (levelDownChance < joker.levelDownChance)
            joker.levelDownChance = levelDownChance;
    }
}
