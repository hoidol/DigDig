using System.Collections.Generic;
using UnityEngine;

public abstract class MiningMachineItemBase : Item
{
    public MiningMachine machinePrefab;

    protected List<MiningMachine> machines = new();

    public override void UpdateItem()
    {
        foreach (var m in machines) Destroy(m.gameObject);
        machines.Clear();

        for (int i = 0; i < count; i++)
        {
            var machine = Instantiate(machinePrefab);
            machine.attackPower = GetAttackPower();
            machines.Add(machine);
        }
    }

    public override void OnUnequip(Player player)
    {
        foreach (var m in machines) Destroy(m.gameObject);
        machines.Clear();
    }

    protected virtual float GetAttackPower()
    {
        return Player.Instance.playerStatMgr.AttackPower * 0.5f;
    }
}
