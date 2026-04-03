public class ShieldEffect : StatusEffect
{
    public override string EffectKey => "Shield";

    // 지속시간 없이 1회 막으면 해제 → duration 무한대
    public ShieldEffect()
    {
        duration       = float.MaxValue;
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
    }
}
