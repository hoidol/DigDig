using UnityEngine;

public class PlayerTakeDamageText : WorldTextBase<PlayerTakeDamageText>
{
    public static void SetText(Vector2 point, string text)
    {
        Show(point, text, GameSetting.enemyDamageColor, "UI/PlayerTakeDamageText");
    }

}

