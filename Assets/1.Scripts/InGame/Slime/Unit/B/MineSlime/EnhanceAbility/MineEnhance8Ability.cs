using UnityEngine;

public class MineEnhance8Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // [각성] 지뢰를 2개씩 설치
    public int minesPerThrow = 2;
    public void Force(Slime slime)
    {
        MineSlime mine = slime as MineSlime;
        mine.minesPerThrow = Mathf.Max(mine.minesPerThrow, minesPerThrow);
    }
}
