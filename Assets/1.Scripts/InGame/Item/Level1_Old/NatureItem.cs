using UnityEngine;


public class NatureItem : Item
{
    //회복력 초당 0.3
    float recoveryHp =  0.3f;
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

        float addRecoveryHp =  Character.Instance.itemInventory.GetItem(key).count *recoveryHp;
        //초당 회복력
        recoveryHpBuff = new Buff(StatType.RecoveryHp, addRecoveryHp, StatOpType.Add);
        Character.Instance.AddBuff(recoveryHpBuff);
    }


    void Release()
    {

        if (recoveryHpBuff != null)
            Character.Instance.RemoveBuff(recoveryHpBuff);
    }


    public override string GetDescription()
    {
        return $"초당 회복력 +{recoveryHp}";
        // return string.Format(TranslateManager.GetText($"{key}_Desc"),maxHp,recoveryHp);
    }
}