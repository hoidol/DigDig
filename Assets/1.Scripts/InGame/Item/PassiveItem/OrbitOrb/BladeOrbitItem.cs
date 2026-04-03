// orbPrefab → OrbitOrb 컴포넌트 달린 칼날 프리팹 연결
public class BladeOrbitItem : OrbitItemBase
{
    public override void OnEquip(Player player) => UpdateItem();
}
