using UnityEngine;

public class HealText : WorldTextBase<HealText>
{
    public static void SetText(Vector2 point, string text)
    {
        Show(point, text, "UI/HealText");
    }
}
