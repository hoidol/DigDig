// 타겟 방식(가까운/먼/랜덤/낮은 체력…) 순환 버튼
// 타겟을 쓰지 않는 슬라임(회전체·경로 순환 등, Minion.UseFindTarget = false)에게는 숨김
public class SetFindTargetButton : AllyOptionButton
{
    public override bool IsAvailable(IAllyUnit unit)
    {
        return unit is not Minion minion || minion.UseFindTarget;
    }

    protected override void SetNext()
    {
        int nextFindTargetTypeIdx = ((int)targetAlly.FindTargetType + 1) % (int)FindTargetType.Count;
        targetAlly.SetFindTargetType((FindTargetType)nextFindTargetTypeIdx);
    }

    protected override string GetText()
    {
        switch (targetAlly.FindTargetType)
        {
            case FindTargetType.Closest: return TranslateManager.GetText("FindTarget.Closest");
            case FindTargetType.Farthest: return TranslateManager.GetText("FindTarget.Farthest");
            case FindTargetType.Random: return TranslateManager.GetText("FindTarget.Random");
            case FindTargetType.LowHp: return TranslateManager.GetText("FindTarget.LowHp");
            default: return targetAlly.FindTargetType.ToString();
        }
    }
}
