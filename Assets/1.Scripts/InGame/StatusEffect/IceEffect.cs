// 감속은 SlowEffect와 같고 이펙트만 다름. 중첩 한도도 슬로우와 합쳐서 계산
public class IceEffect : SlowEffect
{
    public override string EffectKey => "Ice";

    public IceEffect(float duration) : base(duration) { }
}
