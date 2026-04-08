using UnityEngine;

public class DamageText : WorldTextBase<DamageText>
{
    public static void SetText(Vector2 point, string text)
    {
        Show(point, text, "UI/DamageText");
    }
}
