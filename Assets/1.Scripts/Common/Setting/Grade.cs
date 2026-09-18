using UnityEngine;

public class Grade
{
    
    public static Color GetGradeColor(GradeType grade)
    {
        switch (grade)
        {
            case GradeType.D:
                return Color.white;
            case GradeType.C:
                return Color.green;
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
            case GradeType.D:
                return "베이스";
            case GradeType.C:
                return "노말";
            case GradeType.B:
                return "레어";
            case GradeType.A:
                return "유니크";
            case GradeType.S:
                return "레전드";
            case GradeType.SS:
                return "신화"; 
            case GradeType.SSS: 
                return "초월";// Transcendence
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
    D, C, B, A, S, SS, SSS
}
