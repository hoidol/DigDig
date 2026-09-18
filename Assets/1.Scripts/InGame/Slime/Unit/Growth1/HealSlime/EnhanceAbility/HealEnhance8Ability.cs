using UnityEngine;

public class HealEnhance8Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
     // [각성] 캐릭터 최대 체력의 2프로만큼 추가 회복
     public float percent = 0.02f;
    public void Force(Slime slime)
    {
        (slime as HealSlime).healCoolTime *= 0.95f;
    }
}