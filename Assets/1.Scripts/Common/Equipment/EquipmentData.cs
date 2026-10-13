using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu]
public class EquipmentData : ScriptableObject
{
    public string key;
    public EquipmentType equipmentType;
    public EquipPartType equipPartType;
    public Sprite thum;
    public GradeType grade;

    [Header("최대 6개까지 설정하기")]
    public EquipmentAbility[] abilities;

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

#if UNITY_EDITOR
    const int MAX_ABILITY_COUNT = 6;

    public void Edit()
    {
        TableReader table = TableReader.Load("EquipmentData.csv");
        if (table == null) return;

        TableReader.Row row = table.Find("Key", key);
        if (row == null)
        {
            Debug.LogWarning($"[EquipmentData] CSV에서 key '{key}' 를 찾지 못함");
            return;
        }

        if (row.TryGetEnum("EquipmentType", out EquipmentType et)) equipmentType = et;
        if (row.TryGetEnum("EquipPartType", out EquipPartType ept)) equipPartType = ept;
        if (row.TryGetEnum("Grade", out GradeType gr)) grade = gr;

        var abilityList = new System.Collections.Generic.List<EquipmentAbility>();
        for (int s = 0; s < MAX_ABILITY_COUNT; s++)
        {
            if (!row.TryGetEnum($"StatType{s + 1}", out StatType st)) continue;

            row.TryGetFloat($"StatValue{s + 1}", out float value);
            abilityList.Add(new EquipmentAbility { statType = st, value = value });
        }
        abilities = abilityList.ToArray();

        EditorUtility.SetDirty(this);
        Debug.Log($"[EquipmentData] {key} Edit 완료");
    }
#endif
}

public enum EquipmentType
{
    R_Hand, L_Hand, Head, Accessory
}

public enum EquipPartType
{
    R_Hand, L_Hand, Hat, Helmet, Face,
}


/*
AttackPower, //float
MaxHp, //float
RecoveryHp, //float 초당 얼마나 회복될지
AttackSpeed, //float 1초동안 몇발 쏘는지
MoveSpeed, //float 1초동안 얼만큼 가는지
CritChance, //float
CritPower, //float
Dodge
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
