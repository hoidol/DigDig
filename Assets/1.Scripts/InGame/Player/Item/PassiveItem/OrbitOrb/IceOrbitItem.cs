// orbPrefab → IceOrb 컴포넌트 달린 프리팹 연결
public class IceOrbitItem : OrbitItemBase
{
    public override void OnEquip(Player player) => UpdateItem();
}
