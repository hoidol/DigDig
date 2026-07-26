using UnityEngine;

public class CRTDamageText : WorldTextBase<CRTDamageText>
{
    public static void SetText(Vector2 point, string text, Color color)
    {
        Show(point, text, color, "UI/CRTDamageText");
    }
}
