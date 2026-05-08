using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
public class ExpText : WorldTextBase<ExpText>
{
    public static void SetText(Vector2 point, string text)
    {
        Show(point, $"EXP +{text}", "UI/ExpText");
    }

}
