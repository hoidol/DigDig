// 진흙: 적중마다 중첩 +1, 재적용 시 시간 갱신. 중첩당 이동속도 SLOW_PER_STACK(15%) 감소
// 최대 감속: 일반 MAX_SLOW(80%), 엘리트·보스 MAX_SLOW_BIG(60%). 멈추지는 않음
// 마지막 적중 후 duration 지나면 중첩/감속 전부 사라짐. 슬로우·빙결(SlowStack)과는 별개로 곱해짐 (StatusEffectHandler.SlowRate)
public class MudEffect : StatusEffect
{
    public const float SLOW_PER_STACK = 0.15f;
    public const float MAX_SLOW = 0.8f;
    public const float MAX_SLOW_BIG = 0.6f;

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

        Enemy enemy = handler.GetComponent<Enemy>();
        float maxSlow = enemy is EliteEnemy || enemy is Boss ? MAX_SLOW_BIG : MAX_SLOW;
        int maxStack = UnityEngine.Mathf.CeilToInt(maxSlow / SLOW_PER_STACK); // 최대 감속에 닿으면 더 안 쌓음
        if (stackCount < maxStack)
            stackCount++;

        handler.MudSlow = UnityEngine.Mathf.Min(stackCount * SLOW_PER_STACK, maxSlow);
    }

    public override void OnRemove(StatusEffectHandler handler)
    {
        stackCount = 0;
        handler.MudSlow = 0f;
    }
}
