// [프리즘]
// OrbitItemBase. Prism 오브젝트(count개)가 플레이어 주변을 회전.
// PlayerBullet이 프리즘에 닿으면 ±spreadAngle 방향의 AllyBullet 2발로 분열 (데미지 damageRate%).
// 활성 시간 10/15/20초.
public class PrismItem : OrbitItemBase
{
    public float damageRate = 0.5f;
    public float[] activeTimes = { 10, 15, 20 };


    public override void OnActivate() { }
    public override void OnDeactivate() { }

    public override void UpdateItem()
    {
        base.UpdateItem();
        activeTime = activeTimes[count - 1];

        foreach (var orb in orbs)
        {
            if (orb is Prism prism)
                prism.Init(damageRate);
        }
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        return $"주변 프리즘이 총알을 좌우로 분열시킵니다. (분열 데미지 {damageRate * 100:0}%)";
    }
}
