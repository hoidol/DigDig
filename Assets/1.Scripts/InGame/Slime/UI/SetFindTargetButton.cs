using UnityEngine;
using TMPro;

public class SetFindTargetButton : MonoBehaviour
{
    public TMP_Text targetInfoText;
    Slime targetSlime;
    public void SetSlime(Slime slime)
    {
        targetSlime = slime;
        UpdateText();
    }

    void SetFindTargetType(FindTargetType type)
    {
        targetSlime.SetFindTargetType(type);
        UpdateText();
    }

    //다음 타겟 방식으로 순환
    public void OnClickedButton()
    {
        if (targetSlime == null)
            return;

        int nextFindTargetTypeIdx = ((int)targetSlime.findTargetType + 1) % (int)FindTargetType.Count;

        SetFindTargetType((FindTargetType)nextFindTargetTypeIdx);
    }

    void UpdateText()
    {
        if (targetInfoText == null || targetSlime == null)
            return;

        targetInfoText.text = GetFindTargetTypeName(targetSlime.findTargetType);
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
