// machinePrefab → MiningLaser 컴포넌트 + LineRenderer 달린 프리팹 연결
public class MiningLaserItem : MiningMachineItemBase
{
    public override void OnEquip(Player player) => UpdateItem();
}
