using UnityEngine;

public class FlameEnhance5Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    //화염 지속 시간 burnDuration 4 -> 5초
    public void Force(Slime slime)
    {
        (slime as FlameSlime).burnDuration = 5;
    }
}