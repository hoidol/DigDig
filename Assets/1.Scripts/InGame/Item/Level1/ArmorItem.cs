// 갑옷: 최대 체력 +20
public class ArmorItem : Item
{
    float hp = 20;
    Buff maxHpBuff;
    public override void UpdateItem()
    {
        base.UpdateItem();
        Release();
        float addMaxHp =  hp;
        //최대 체력
        maxHpBuff = new Buff(StatType.MaxHp, addMaxHp, StatOpType.Add);
        Character.Instance.AddBuff(maxHpBuff);

    }
    void Release()
    {
        if (maxHpBuff != null)
            Character.Instance.RemoveBuff(maxHpBuff);
    }


    public override string GetDescription()
    {
        return $"최대 체력 +{hp}";
        //return string.Format(TranslateManager.GetText("{key}_Desc"),hp);
    }
}
