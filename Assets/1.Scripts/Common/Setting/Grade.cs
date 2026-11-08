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


public enum GradeType : int
{
    C, B, A, S, SS, SSS
}
