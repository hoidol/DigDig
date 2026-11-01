// 적중할 때마다 슬로우 중첩 +1 (효과 하나당 최대 MAX_SLOW_STACK), 재적용 시 시간 갱신. 끝나면 쌓은 중첩을 한 번에 해제
public class SlowEffect : StatusEffect
{
    int stackCount;

    public SlowEffect(float duration)
    {
        this.duration = duration;
        this.remainingTimer = duration;
    }

    public override void OnApply(StatusEffect effect, StatusEffectHandler handler)
    {
        if (effect != this && remainingTimer < effect.duration)
            remainingTimer = effect.duration;

        if (stackCount >= StatusEffectHandler.MAX_SLOW_STACK)
            return;

        stackCount++;
        handler.SlowStack++;
    }

    public override void OnRemove(StatusEffectHandler handler)
    {
        handler.SlowStack -= stackCount;
        stackCount = 0;
    }
}
