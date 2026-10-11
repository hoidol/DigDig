using UnityEngine;
using TMPro;

public class SetFindTargetButton : MonoBehaviour
{
    public TMP_Text targetInfoText;
    IAllyUnit targetAlly;
    public void SetSlime(IAllyUnit slime)
    {
        targetAlly = slime;
        UpdateText();
    }

    void SetFindTargetType(FindTargetType type)
    {
        targetAlly.SetFindTargetType(type);
        UpdateText();
    }

    //다음 타겟 방식으로 순환
    public void OnClickedButton()
    {
        if (targetAlly == null)
            return;

        int nextFindTargetTypeIdx = ((int)targetAlly.FindTargetType + 1) % (int)FindTargetType.Count;

        SetFindTargetType((FindTargetType)nextFindTargetTypeIdx);
    }

    void UpdateText()
    {
        if (targetInfoText == null || targetAlly == null)
            return;

        targetInfoText.text = GetFindTargetTypeName(targetAlly.FindTargetType);
    }

    string GetFindTargetTypeName(FindTargetType type)
    {
        switch (type)
        {
            case FindTargetType.Closest: return "가까운 적";
            case FindTargetType.Farthest: return "먼 적";
            case FindTargetType.Random: return "랜덤";
            case FindTargetType.LowHp: return "낮은 체력";
            default: return type.ToString();
        }
    }
}
