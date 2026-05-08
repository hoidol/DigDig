// [화염 궤도]
// OrbitItemBase. FlameOrb(count개)가 플레이어 주변을 회전하며 적에게 피해 및 화상 상태이상 적용.
// orbPrefab에 FlameOrb 컴포넌트 달린 프리팹 연결.
public class FlameOrbitItem : OrbitItemBase
{
    public override void OnActivate()
    {

    }

    public override void OnDeactivate()
    {

    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"화염구 {c}개가 주변을 회전하며 적에게 피해 및 화상 적용";
    }
}
