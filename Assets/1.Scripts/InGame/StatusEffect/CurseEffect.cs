// (2026-11-25) 저주 (사령술사 2단계 스킬): duration초 동안 받는 피해 +damageBonus (모든 아군 공격, 낙인과 곱함)
// 다시 걸리면 증가량은 큰 값, 남은 시간은 긴 쪽. 표시는 낙인과 같은 보라색 표시(Stigma)를 씀
public class CurseEffect : StatusEffect
{
    public float damageBonus; // 0.5 = 50%
    public override string EffectKey => "Stigma";

    public CurseEffect(float duration, float damageBonus)
    {
        this.duration = duration;
        remainingTimer = duration;
        this.damageBonus = damageBonus;
    }

    public override void OnApply(StatusEffect effect, StatusEffectHandler handler)
    {
        if (effect is CurseEffect incoming && incoming != this)
        {
            if (incoming.damageBonus > damageBonus)
                damageBonus = incoming.damageBonus;
            if (incoming.remainingTimer > remainingTimer)
                remainingTimer = incoming.remainingTimer;
        }
        handler.CurseDamageMul = 1f + damageBonus;
    }

    public override void OnRemove(StatusEffectHandler handler)
    {
        handler.CurseDamageMul = 1f;
    }
}
