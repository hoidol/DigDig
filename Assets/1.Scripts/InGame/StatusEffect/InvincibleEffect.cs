// 무적 상태 이펙트 - IsShielded를 유지해 TryBlock이 항상 true 반환하도록 함
public class InvincibleEffect : StatusEffect
{
    public InvincibleEffect()
    {
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

    // 외부에서 무적 해제 시 호출
    public void Deactivate() => remainingTimer = 0;
}
