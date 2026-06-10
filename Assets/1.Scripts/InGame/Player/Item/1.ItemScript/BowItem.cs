using UnityEngine;

// 공격력 25% 증가
public class BowItem : Item
{
    Buff buff;
    const float buffValue =1.3f; 

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.AttackPower, buffValue, StatOpType.Multiply);
        player.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }

}
