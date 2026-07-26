using UnityEngine;

public class DamageText : WorldTextBase<DamageText>
{
    public static void SetText(Vector2 point, string text, Color color)
    {
        Show(point, text, color, "UI/DamageText");
    }
}
