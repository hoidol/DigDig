using UnityEngine;


public class NatureRune : Rune
{
    //회복력 초당 0.3
    float recoveryHp =  0.3f;
    Buff recoveryHpBuff;

    public override void OnUnequip()
    {
        base.OnUnequip();
        Release();

    }
    public override void UpdateRune()
    {
        base.UpdateRune();
        Release();

        float addRecoveryHp =  recoveryHp;
        //초당 회복력
        // recoveryHpBuff = new Buff(StatType.RecoveryHp, addRecoveryHp, StatOpType.Add);
        Slime.Instance.AddBuff(recoveryHpBuff);
    }


    void Release()
    {

        if (recoveryHpBuff != null)
            Slime.Instance.RemoveBuff(recoveryHpBuff);
    }


    public override string GetDescription()
    {
        return $"초당 회복력 +{recoveryHp}";
        // return string.Format(TranslateManager.GetText($"{key}_Desc"),maxHp,recoveryHp);
    }
}