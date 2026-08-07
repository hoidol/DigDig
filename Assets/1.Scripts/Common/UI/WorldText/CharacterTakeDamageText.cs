using UnityEngine;

public class CharacterTakeDamageText : WorldTextBase<CharacterTakeDamageText>
{
    public static void SetText(Vector2 point, string text)
    {
        Show(point, text, GameSetting.enemyDamageColor, "UI/CharacterTakeDamageText");
    }

}

