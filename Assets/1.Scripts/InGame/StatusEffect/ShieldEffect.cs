using System;

public class ShieldEffect : StatusEffect
{
    public override string EffectKey => "Shield";
    Action consumeListener;

    // 지속시간 없이 1회 막으면 해제 → duration 무한대
    public ShieldEffect(Action cListener)
    {
        consumeListener = cListener;
        duration = float.MaxValue;
        remainingTimer = float.MaxValue;
    }

    public override void OnApply(StatusEffect effect, StatusEffectHandler handler)
    {
        handler.IsShielded = true;
    }

    public override void OnRemove(StatusEffectHandler handler)
    {
        handler.IsShielded = false;
    }

    // 외부에서 방어막 소모 시 호출
    public void Consume(StatusEffectHandler handler)
    {
        remainingTimer = 0; // IsExpired → true → 다음 Update에서 OnRemove
        consumeListener?.Invoke();
    }
}
