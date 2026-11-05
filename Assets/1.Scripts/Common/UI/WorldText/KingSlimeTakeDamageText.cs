using UnityEngine;

public class KingSlimeTakeDamageText : WorldTextBase<KingSlimeTakeDamageText>
{
    public static void SetText(Vector2 point, string text)
    {
        Show(point, text, ColorSetting.enemyDamageColor, "UI/KingSlimeTakeDamageText");
    }

}

