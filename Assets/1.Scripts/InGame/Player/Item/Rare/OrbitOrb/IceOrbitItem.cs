// [얼음 궤도]
// OrbitItemBase. IceOrb(count개)가 플레이어 주변을 회전하며 적에게 피해 및 빙결 상태이상 적용.
// orbPrefab에 IceOrb 컴포넌트 달린 프리팹 연결.
public class IceOrbitItem : OrbitItemBase
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
        return $"얼음구 {c}개가 주변을 회전하며 적에게 피해 및 빙결 적용";
    }
}
