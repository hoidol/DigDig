using System;
using UnityEngine;
#if UNITY_EDITOR
using System.Globalization;
using System.IO;
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
        string path = Path.Combine(Application.dataPath, "Json/EquipmentData.csv");
        if (!File.Exists(path)) { Debug.LogWarning($"[EquipmentData] CSV 없음: {path}"); return; }

        string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
        if (lines.Length < 2) return;

        string[] headers = lines[0].Split('\t');
        for (int i = 0; i < headers.Length; i++) headers[i] = headers[i].Trim();

        int iKey = Array.IndexOf(headers, "key");
        int iEquipmentType = Array.IndexOf(headers, "equipmentType");
        int iEquipPartType = Array.IndexOf(headers, "equipPartType");
        int iGrade = Array.IndexOf(headers, "grade");

        int[] iStatType = new int[MAX_ABILITY_COUNT];
        int[] iStatValue = new int[MAX_ABILITY_COUNT];
        for (int s = 0; s < MAX_ABILITY_COUNT; s++)
        {
            iStatType[s] = Array.IndexOf(headers, $"statType{s + 1}");
            iStatValue[s] = Array.IndexOf(headers, $"statValue{s + 1}");
        }

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');

            if (Col(cols, iKey) != key) continue;

            if (Enum.TryParse<EquipmentType>(Col(cols, iEquipmentType), out var et)) equipmentType = et;
            if (Enum.TryParse<EquipPartType>(Col(cols, iEquipPartType), out var ept)) equipPartType = ept;
            if (Enum.TryParse<GradeType>(Col(cols, iGrade), out var gr)) grade = gr;

            var abilityList = new System.Collections.Generic.List<EquipmentAbility>();
            for (int s = 0; s < MAX_ABILITY_COUNT; s++)
            {
                string typeStr = Col(cols, iStatType[s]);
                if (string.IsNullOrEmpty(typeStr)) continue;
                if (!Enum.TryParse<StatType>(typeStr, out var st)) continue;

                float.TryParse(Col(cols, iStatValue[s]), NumberStyles.Float, CultureInfo.InvariantCulture, out float value);
                abilityList.Add(new EquipmentAbility { statType = st, value = value });
            }
            abilities = abilityList.ToArray();

            EditorUtility.SetDirty(this);
            Debug.Log($"[EquipmentData] {key} Edit 완료");
            return;
        }

        Debug.LogWarning($"[EquipmentData] CSV에서 key '{key}' 를 찾지 못함");
    }

    static string Col(string[] cols, int idx) => idx >= 0 && idx < cols.Length ? cols[idx].Trim() : "";
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
