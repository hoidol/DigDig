using UnityEngine;

public class CRTDamageText : WorldTextBase<CRTDamageText>
{
    public static void SetText(Vector2 point, string text)
    {
        Show(point, text, "UI/CRTDamageText");
    }
}
