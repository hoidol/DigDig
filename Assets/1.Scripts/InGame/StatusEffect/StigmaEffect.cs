// 낙인: 걸리면 받는 피해 +damageBonus (모든 아군 공격에 적용). 중첩 없음 (2026-11-16, 이전 최대 3중첩)
// 적이 죽을 때까지 유지 (시간 만료 없음). 이미 낙인이면 피해 증가량만 큰 값으로 갱신 (진화한 낙인 미니언 등)
public class StigmaEffect : StatusEffect
{
    public float damageBonus; // 0.15 = 15%
    public override string EffectKey => "Stigma"; // 적 프리팹 StatusEffect 하위 Stigma 표시 (StatusEffectView)

    public StigmaEffect(float damageBonus)
    {
        this.damageBonus = damageBonus;
        duration = float.MaxValue;
        remainingTimer = float.MaxValue;
    }

    public override void OnApply(StatusEffect effect, StatusEffectHandler handler)
    {
        if (effect is StigmaEffect incoming && incoming != this && incoming.damageBonus > damageBonus)
            damageBonus = incoming.damageBonus;

        handler.StigmaDamageMul = 1f + damageBonus;
    }

    public override void OnRemove(StatusEffectHandler handler)
    {
        handler.StigmaDamageMul = 1f;
    }
}
