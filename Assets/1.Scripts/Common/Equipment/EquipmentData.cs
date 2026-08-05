using UnityEngine;


public class EquipmentData : ScriptableObject 
{
    public string key;
    public EquipmentType equipmentType;
    public EquipPositionType equipPositionType;
    public Sprite thum;
    public EquipmentGrade grade;

    public static Color GetGradeColor(EquipmentGrade grade)
    {
        switch (grade)
        {
            case EquipmentGrade.D:
                return Color.white;
            case EquipmentGrade.C:
                return Color. green;
            case EquipmentGrade.B:
                return Color.blue;
            case EquipmentGrade.A:
                return Color.purple;
            case EquipmentGrade.S:
                return Color.magenta;
            case EquipmentGrade.SS:
                return new Color(1f, 0.5f, 0f); // Orange
            case EquipmentGrade.SSS:
                return Color.red;
            default:
                return Color.white;
        }
    }
}

public enum EquipmentType
{
    Weapon, Shield, Helmet, Accessory
}

public enum EquipPositionType
{
    RightHand, LeftHand, Head, Face,
}

public enum EquipmentGrade: int
{
    D, C, B, A,S,SS,SSS
}
//공격력, 체력, 치명타 확률, 치명타 피해량, 이동속도, 공격속도, 바운스