using UnityEngine;

public class SlimeTakeDamageText : WorldTextBase<SlimeTakeDamageText>
{
    public static void SetText(Vector2 point, string text)
    {
        Show(point, text, ColorSetting.enemyDamageColor, "UI/SlimeTakeDamageText");
    }

}

