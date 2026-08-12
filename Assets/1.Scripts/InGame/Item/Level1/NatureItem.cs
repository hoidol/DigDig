using UnityEngine;


public class NatureItem : Item
{
    //체력 +10, 회복력 초당 0.3
    float maxHp =  10;
    float recoveryHp =  0.3f;
    Buff maxHpBuff;
    Buff recoveryHpBuff;

    public override void OnUnequip()
    {
        base.OnUnequip();
        Release();

    }
    public override void UpdateItem()
    {
        base.UpdateItem();
        Release();


        float addHp =  Character.Instance.itemInventory.GetItem(key).count *maxHp;
        //체력
        maxHpBuff = new Buff(StatType.MaxHp, addHp, StatOpType.Add);
        Character.Instance.AddBuff(maxHpBuff);


        float addRecoveryHp =  Character.Instance.itemInventory.GetItem(key).count *recoveryHp;
        //초당 회복력
        recoveryHpBuff = new Buff(StatType.RecoveryHp, addRecoveryHp, StatOpType.Add);
        Character.Instance.AddBuff(recoveryHpBuff);
    }


    void Release()
    {
        if (maxHpBuff != null)
            Character.Instance.RemoveBuff(maxHpBuff);

        if (recoveryHpBuff != null)
            Character.Instance.RemoveBuff(recoveryHpBuff);
    }


    public override string GetDescription()
    {
        return $"체력 +{maxHp} 초당 회복력 +{recoveryHp}";
    }
}