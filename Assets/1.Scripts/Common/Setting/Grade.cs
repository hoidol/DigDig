using UnityEngine;

public class Grade
{
    
    public static Color GetGradeColor(GradeType grade)
    {
        switch (grade)
        {
            // case GradeType.D:
            //     return Color.white;
            case GradeType.C:
                return Color.white; //green
            case GradeType.B:
                return Color.blue;
            case GradeType.A:
                return Color.purple;
            case GradeType.S:
                return new Color(1,206f/266f,0);
            case GradeType.SS:
                return new Color(1f, 0.5f, 0f); // Orange
            case GradeType.SSS:
                return Color.red;
            default:
                return Color.white;
        }
    }
    // 등급별 카드 틀 색 (슬라임 칸의 Bg / InnerBorder / HighLight). 등급 라벨(MinionGradePanel) 색과 같은 계열
    public static GradeCardColor GetCardColor(GradeType grade)
    {
        switch (grade)
        {
            case GradeType.B:
                return new GradeCardColor("#344FB4", "#4A6BD6", "#A9BEFF");
            case GradeType.A:
                return new GradeCardColor("#8A4FBA", "#A866DB", "#DDB5FF");
            case GradeType.S:
                return new GradeCardColor("#CAB046", "#E7D34A", "#FFF3A5");
            case GradeType.SS:
                return new GradeCardColor("#BA182E", "#DB3348", "#FFA3AE");
            case GradeType.SSS:
                return new GradeCardColor("#D9661A", "#F58E3A", "#FFD0A3");
            default:
                return new GradeCardColor("#8E8E8E", "#B4B4B4", "#EDEDED");
        }
    }
    public static string GetGradeText(GradeType grade)
    {
        switch (grade)
        {
            case GradeType.C:
                return TranslateManager.GetText("Grade.C");
            case GradeType.B:
                return TranslateManager.GetText("Grade.B");
            case GradeType.A:
                return TranslateManager.GetText("Grade.A");
            case GradeType.S:
                return TranslateManager.GetText("Grade.S");
            case GradeType.SS:
                return TranslateManager.GetText("Grade.SS"); 
            case GradeType.SSS: 
                return TranslateManager.GetText("Grade.SSS");// Transcendence
            default:
                return null;
        }
    }
    public static Sprite GetGradeSprite(GradeType grade)
    {
        string spriteName = $"grade_{grade.ToString().ToLower()}";
        return Resources.Load<Sprite>($"UI/Grade/{spriteName}");
    }
}

public struct GradeCardColor
{
    public Color bg, innerBorder, highLight;
    public GradeCardColor(string bgHex, string innerBorderHex, string highLightHex)
    {
        ColorUtility.TryParseHtmlString(bgHex, out bg);
        ColorUtility.TryParseHtmlString(innerBorderHex, out innerBorder);
        ColorUtility.TryParseHtmlString(highLightHex, out highLight);
    }
}

public enum GradeType : int
{
    C, B, A, S, SS, SSS
}
