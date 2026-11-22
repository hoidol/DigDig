// (2026-11-21) 덩굴 (킹 패시브 엘프 얽히는 덩굴): 이동속도 slow만큼 감소, 중첩 없음(강한 값), 다시 걸리면 시간 갱신
// 슬로우·빙결·진흙과는 곱함 (StatusEffectHandler.SlowRate)
public class VineEffect : StatusEffect
{
    float slow;

    public VineEffect(float slow, float duration)
    {
        this.slow = slow;
        this.duration = duration;
        remainingTimer = duration;
    }

    public override void OnApply(StatusEffect effect, StatusEffectHandler handler)
    {
        if (effect is VineEffect incoming && incoming != this)
        {
            if (incoming.slow > slow)
                slow = incoming.slow;
            if (remainingTimer < incoming.duration)
                remainingTimer = incoming.duration;
        }
        handler.VineSlow = slow;
    }

    public override void OnRemove(StatusEffectHandler handler)
    {
        handler.VineSlow = 0f;
    }
}
