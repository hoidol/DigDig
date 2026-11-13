using UnityEngine;

namespace Lobby
{
    public class DrawMinionResultCanvas : DrawResultCanvas<DrawMinionResultCanvas> 
{
    protected override GradeType SetCardData(DrawResultCard card, string key)
    {
        MinionData minionData = MinionManager.Instance.GetMinionData(key);
        card.GetComponentInChildren<DrawMinionPanel>(true).SetData(minionData);
        return minionData.grade;
    }
}
}
