using System;
using UnityEngine;

[CreateAssetMenu]
public class EquipmentData : ScriptableObject
{
    public string key;
    public EquipmentType equipmentType;
    public EquipPartType equipPartType;
    public Sprite thum;
    public Grade grade;

    [Header("최대 6개까지 설정하기")]
    public EquipmentAbility[] abilities;

    public static Color GetGradeColor(Grade grade)
    {
        switch (grade)
        {
            case Grade.D:
                return Color.white;
            case Grade.C:
                return Color.green;
            case Grade.B:
                return Color.blue;
            case Grade.A:
                return Color.purple;
            case Grade.S:
                return Color.magenta;
            case Grade.SS:
                return new Color(1f, 0.5f, 0f); // Orange
            case Grade.SSS:
                return Color.red;
            default:
                return Color.white;
        }
    }
    public EquipmentAbility GetEquipmentAbility(StatType statType)
    {
        foreach (var ability in abilities)
        {
            if (ability.statType == statType)
            {
                return ability;
            }
        }
        return null;
    }
    public float GetEquipmentAbilityValue(StatType statType)
    {
        foreach (var ability in abilities)
        {
            if (ability.statType == statType)
            {
                return ability.value;
            }
        }
        return 0f;
    }
    public static Sprite GetGradeSprite(Grade grade)
    {
        string spriteName = $"grade_{grade.ToString().ToLower()}";
        return Resources.Load<Sprite>($"UI/Grade/{spriteName}");
    }

    // public static bool RecommandChange(EquipmentData newEquipmentData,  EquipmentData oldEquipmentData)
    // {


    //     float oldValue = float.Parse(oldAbility.value);
    //     float newValue = float.Parse(newAbility.value);

    //     if (oldValue < newValue)
    //         return true;
    //     else
    //         return false;
    // }

    // public int GetEquipmentValue()
    // {

    // }
}

public enum EquipmentType
{
    R_Hand, L_Hand, Head, Accessory
}

public enum EquipPartType
{
    RightHand, LeftHand, Hat, Helmet, Face,
}

public enum Grade : int
{
    D, C, B, A, S, SS, SSS
}


/*
AttackPower, //float
MaxHp, //float
RecoveryHp, //float 초당 얼마나 회복될지
AttackSpeed, //float 1초동안 몇발 쏘는지
MoveSpeed, //float 1초동안 얼만큼 가는지
CritChance, //float
CritPower, //float
Bounce
*/
[System.Serializable]
public class EquipmentAbility
{
    public StatType statType;
    public float value;
    public string Title => statType.ToString();
    public string GetValueToString() => value.ToString("0.#");
    public T GetValue<T>()
    {
        return (T)Convert.ChangeType(value, typeof(T));
    }

    public void AddAbility(EquipmentAbility other)
    {
        if (other == null || other.statType != this.statType)
            return;

        float currentValue = this.value;
        float otherValue = other.value;
        float newValue = currentValue + otherValue;
        this.value = newValue;
    }

}
