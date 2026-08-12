using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// 회복	- 초당 1씩 체력 회복
// 자연	행운
public class RegenItem : Item
{
    float recoverHp =  0.5f;
    Buff recoveryHpBuff;


    public override void OnEquip()
    {
        base.OnEquip();
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        Release();
    }
    public override void UpdateItem()
    {
        base.UpdateItem();
        Release();
        //초당 체력 회복
        recoveryHpBuff = new Buff(StatType.RecoveryHp, recoverHp * count, StatOpType.Add);
        Character.Instance.AddBuff(recoveryHpBuff);
    }

    void Release()
    {
        if (recoveryHpBuff != null)
            Character.Instance.RemoveBuff(recoveryHpBuff);
    }

    public override string GetDescription()
    {
        return $"초당 체력 +{recoverHp}회복 ";
    }
}

