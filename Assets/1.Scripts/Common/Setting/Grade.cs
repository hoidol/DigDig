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
                return Color.magenta;
            case GradeType.SS:
                return new Color(1f, 0.5f, 0f); // Orange
            case GradeType.SSS:
                return Color.red;
            default:
                return Color.white;
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
