// orbPrefab → FlameOrb 컴포넌트 달린 프리팹 연결
public class FlameOrbitItem : OrbitItemBase
{
    public override void OnEquip(Player player) => UpdateItem();
}
