// 가죽 장화: 이동속도 +10 증가
public class LeatherBootsRune : Rune
{
    float moveSpeed = 7;
    Buff moveSpeedBuff;
    public override void UpdateRune()
    {
        base.UpdateRune();
        Release();
        float addMoveSpeed =  moveSpeed;
        //이동속도
        // moveSpeedBuff = new Buff(StatType.MoveSpeed, addMoveSpeed, StatOpType.Add);
        Slime.Instance.AddBuff(moveSpeedBuff);

    }
    void Release()
    {
        if (moveSpeedBuff != null)
            Slime.Instance.RemoveBuff(moveSpeedBuff);
    }


    public override string GetDescription()
    {
        return $"이동속도 +{moveSpeed}";
        //return string.Format(TranslateManager.GetText("{key}_Desc"),moveSpeed);
    }
}
