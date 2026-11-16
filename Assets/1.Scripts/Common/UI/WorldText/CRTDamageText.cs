using UnityEngine;

// 치명타 피해 숫자: 더 크게 + 흔들림 + "!" (약점·내성·피해량 크기도 같이 적용). 연출은 WorldTextBase.Play
public class CRTDamageText : WorldTextBase<CRTDamageText>
{
    public static void SetText(Vector2 point, string text, Color color)
    {
        Show(point, text, color, "UI/CRTDamageText", new WorldTextStyle { size = 1f, crit = true });
    }

    public static void SetText(Vector2 point, string text, float size, DamageTextType type)
    {
        Show(point, text, Color.white, "UI/CRTDamageText", new WorldTextStyle { size = size, crit = true, type = type });
    }
}
