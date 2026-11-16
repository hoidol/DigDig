using UnityEngine;

// 아군이 적에게 준 피해 숫자 (치명타는 CRTDamageText). 연출은 WorldTextBase.Play
public class DamageText : WorldTextBase<DamageText>
{
    public static void SetText(Vector2 point, string text, Color color)
    {
        Show(point, text, color, "UI/DamageText");
    }

    // size = 피해량 비례 크기, type = 약점·내성 표시
    public static void SetText(Vector2 point, string text, float size, DamageTextType type)
    {
        Show(point, text, Color.white, "UI/DamageText", new WorldTextStyle { size = size, type = type });
    }
}
