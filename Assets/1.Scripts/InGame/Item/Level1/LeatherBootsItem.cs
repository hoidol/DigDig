// 가죽 장화: 이동속도 25% 증가
public class LeatherBootsItem : Item
{
    float moveSpeed = 5;
    Buff moveSpeedBuff;
    public override void UpdateItem()
    {
        base.UpdateItem();
        Release();
        float addMoveSpeed =  moveSpeed;
        //이동속도
        moveSpeedBuff = new Buff(StatType.MoveSpeed, addMoveSpeed, StatOpType.Add);
        Character.Instance.AddBuff(moveSpeedBuff);

    }
    void Release()
    {
        if (moveSpeedBuff != null)
            Character.Instance.RemoveBuff(moveSpeedBuff);
    }


    public override string GetDescription()
    {
        return $"이동속도 +{moveSpeed}%";
        //return string.Format(TranslateManager.GetText("{key}_Desc"),moveSpeed);
    }
}
