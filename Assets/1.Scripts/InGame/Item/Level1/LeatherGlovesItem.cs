// 가죽 장갑: 공격속도 25% 증가
public class LeatherGlovesItem : Item
{
    float attackSpeed = 5;
    Buff attackSpeedBuff;
    public override void UpdateItem()
    {
        base.UpdateItem();
        Release();
        float addAttackSpeed =  attackSpeed;
        //공격속도
        attackSpeedBuff = new Buff(StatType.AttackSpeed, addAttackSpeed, StatOpType.Add);
        Character.Instance.AddBuff(attackSpeedBuff);

    }
    void Release()
    {
        if (attackSpeedBuff != null)
            Character.Instance.RemoveBuff(attackSpeedBuff);
    }


    public override string GetDescription()
    {
        return $"공격속도 +{attackSpeed}%";
        //return string.Format(TranslateManager.GetText("{key}_Desc"),attackSpeed);
    }
}
