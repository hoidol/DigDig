// 낙인: 적중마다 중첩 +1 (최대 maxStack). 중첩당 받는 피해 +damagePerStack (모든 아군 공격에 적용)
// 적이 죽을 때까지 유지 (시간 만료 없음). 강화가 다른 낙인 슬라임이 여럿이면 최대 중첩/중첩당 피해는 큰 값을 쓴다
public class StigmaEffect : StatusEffect
{
    public int maxStack;
    public float damagePerStack; // 0.05 = 5%
    int stackCount;

    public StigmaEffect(int maxStack, float damagePerStack)
    {
        this.maxStack = maxStack;
        this.damagePerStack = damagePerStack;
        duration = float.MaxValue;
        remainingTimer = float.MaxValue;
    }

    public override void OnApply(StatusEffect effect, StatusEffectHandler handler)
    {
        if (effect is StigmaEffect incoming && incoming != this)
        {
            if (incoming.maxStack > maxStack) maxStack = incoming.maxStack;
            if (incoming.damagePerStack > damagePerStack) damagePerStack = incoming.damagePerStack;
        }

        if (stackCount < maxStack)
            stackCount++;

        handler.StigmaStack = stackCount;
        handler.StigmaDamageMul = 1f + stackCount * damagePerStack;
    }

    public override void OnRemove(StatusEffectHandler handler)
    {
        stackCount = 0;
        handler.StigmaStack = 0;
        handler.StigmaDamageMul = 1f;
    }
}
