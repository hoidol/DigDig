using UnityEngine;

namespace Lobby
{
    public class DrawSlimeResultCanvas : DrawResultCanvas<DrawSlimeResultCanvas> 
{
    protected override GradeType SetCardData(DrawResultCard card, string key)
    {
        SlimeData slimeData = SlimeManager.Instance.GetSlimeData(key);
        card.GetComponentInChildren<DrawSlimePanel>(true).SetData(slimeData);
        return slimeData.grade;
    }
}
}
