using UnityEngine;

public class HealText : WorldTextBase<HealText>
{
    public static void SetText(Vector2 point, string text, Color color)
    {
        Show(point, text, color, "UI/HealText");
    }
}
