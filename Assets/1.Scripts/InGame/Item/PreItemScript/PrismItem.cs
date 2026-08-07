// [프리즘]
// OrbitItemBase. Prism 오브젝트가 플레이어 주변을 회전.
// PlayerBullet이 프리즘에 닿으면 ±spreadAngle 방향의 AllyBullet 2발로 분열 (데미지 50%).
public class PrismItem : OrbitItemBase
{
    const float DAMAGE_RATE = 0.5f;

    public override void OnActivate() { }
    public override void OnDeactivate() { }

    public override void UpdateItem()
    {
        base.UpdateItem();
        foreach (var orb in orbs)
        {
            if (orb is Prism prism)
                prism.Init(DAMAGE_RATE);
        }
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"주변 프리즘이 총알을 좌우로 분열시킵니다. (분열 데미지 {DAMAGE_RATE * 100:0}%)";
    }
}
