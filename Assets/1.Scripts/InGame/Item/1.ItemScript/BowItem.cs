using UnityEngine;

// 공격력 25% 증가
public class BowItem : Item
{
    Buff buff;
    float[] buffValues ={1.3f,1.45f,1.6f}; 


    public override void UpdateItem()
    {
        if(buff != null)
            Character.Instance.RemoveBuff(buff);

        buff = new Buff(StatType.AttackPower, buffValues[count-1], StatOpType.Multiply);
    }
    public override void OnUnequip()
    {
        Character.Instance.RemoveBuff(buff);
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"공격력 {(buffValues[lv-1] -1 )* 100}% 증가";
    }

}
