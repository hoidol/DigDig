// 진흙: 적중마다 중첩 +1 (최대 MAX_STACK = 15), 재적용 시 시간 갱신. 중첩당 이동속도 SLOW_PER_STACK(5%) 감소 → 최대 75%
// 엘리트·보스도 같은 규칙 (2026-11-16, 이전: 중첩당 15%, 최대 80% / 엘리트·보스 60%). 멈추지는 않음
// 마지막 적중 후 duration 지나면 중첩/감속 전부 사라짐. 슬로우·빙결(SlowStack)과는 별개로 곱해짐 (StatusEffectHandler.SlowRate)
public class MudEffect : StatusEffect
{
    public const float SLOW_PER_STACK = 0.05f;
    public const int MAX_STACK = 15;
    public const float MAX_SLOW = SLOW_PER_STACK * MAX_STACK;

    int stackCount;

    public MudEffect(float duration)
    {
        this.duration = duration;
        this.remainingTimer = duration;
    }

    public override void OnApply(StatusEffect effect, StatusEffectHandler handler)
    {
        if (effect != this && remainingTimer < effect.duration)
            remainingTimer = effect.duration;

        if (stackCount < MAX_STACK)
            stackCount++;

        handler.MudSlow = stackCount * SLOW_PER_STACK;
    }

    public override void OnRemove(StatusEffectHandler handler)
    {
        stackCount = 0;
        handler.MudSlow = 0f;
    }
}
