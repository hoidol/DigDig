// 십자 버프 슬라임 공용: [각성] 포위 진형 - 십자 4칸이 모두 슬라임으로 차 있으면 버프 효과 +50%
// (같은 TileGroup 안 십자 4칸이 다 있어야 함 → 3*3 그룹 가운데 칸 + 막힌 칸 없음. CrossBuffSlime.IsSurrounded)
public class CrossBuffSurroundEnhanceAbility : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    public float bonus = 0.5f;
    public void Force(Slime slime)
    {
        (slime as CrossBuffSlime).surroundBonus += bonus;
    }
}
