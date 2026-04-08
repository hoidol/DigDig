// machinePrefab → MiningDrone 컴포넌트 달린 드론 프리팹 연결
public class MiningDroneItem : MiningMachineItemBase
{
    public override void OnEquip(Player player) => UpdateItem();
}
