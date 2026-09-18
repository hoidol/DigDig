using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public abstract class EnhanceAbilitySlimeData : SlimeData
{
    public EnhanceAbilityInfo[] enhanceAbilityInfos;

    // commonSlimeEnhanceInfos: 강화 레벨(1~15)마다 달라지는 공용 스탯(AttackPower/AttackSpeed/AttackRange) + EnhanceAbility 설명
    // uniqueSlimeStats: FlameDPS, FlameDuration처럼 강화 레벨에 상관없이 고정인 슬라임 고유 스탯 (머지레벨 0~2 값만 가짐)
    public SlimeStat[] uniqueSlimeStats;
    public SlimeStat GetUniqueStat(SlimeStatType statType)
    {
        return uniqueSlimeStats.FirstOrDefault(e => e.statType == statType);
    }


    public override SlimeStat GetSlimeStat(SlimeStatType statType, int enhanceLv)
    {
        SlimeStat slimeStat = base.GetSlimeStat(statType,enhanceLv);
        if(slimeStat == null)
        {
            return GetUniqueStat(statType);
        }

        return slimeStat;
    }

#if UNITY_EDITOR
    // csv 공용 컬럼(level, AttackPower, AttackSpeed, AttackRange, EnhanceAbility)을 읽어
    // commonSlimeEnhanceInfos, enhanceAbilityInfos 배열을 채운다.
    // 슬라임별 고유 스탯(FlameDPS 등)은 자식 클래스에서 SetUniqueStat으로 uniqueSlimeStats에 설정한다.
    public override void LoadData()
    {
        base.LoadData();

        string path = System.IO.Path.Combine(Application.dataPath, $"Json/SlimeEnhance/{key}.csv");
        if (!System.IO.File.Exists(path))
        {
            CreateTempCsv(path);
        }

        string[] lines = System.IO.File.ReadAllLines(path, System.Text.Encoding.UTF8);
        if (lines.Length < 2) return;

        string[] headers = ParseCsvLine(lines[0]);
        for (int i = 0; i < headers.Length; i++)
            headers[i] = headers[i].Trim();

        int iLevel = System.Array.IndexOf(headers, "level");
        int iAttackPower = System.Array.IndexOf(headers, "AttackPower");
        int iAttackSpeed = System.Array.IndexOf(headers, "AttackSpeed");
        int iAttackRange = System.Array.IndexOf(headers, "AttackRange");
        int iEnhanceAbility = System.Array.IndexOf(headers, "EnhanceAbility");

        var enhanceInfos = new List<SlimeEnhanceInfo>();
        var abilityInfos = new List<EnhanceAbilityInfo>();

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = ParseCsvLine(lines[i]);

            if (!int.TryParse(GetCol(cols, iLevel), out int level))
                continue;

            var stats = new List<SlimeStat>();
            AddStat(stats, SlimeStatType.AttackPower, GetCol(cols, iAttackPower));
            AddStat(stats, SlimeStatType.AttackSpeed, GetCol(cols, iAttackSpeed));
            AddStat(stats, SlimeStatType.AttackRange, GetCol(cols, iAttackRange));
            enhanceInfos.Add(new SlimeEnhanceInfo { level = level, slimeStats = stats.ToArray() });

            string desc = GetCol(cols, iEnhanceAbility);
            if (!string.IsNullOrWhiteSpace(desc))
                abilityInfos.Add(new EnhanceAbilityInfo { level = level, desc = desc });
        }

        commonSlimeEnhanceInfos = enhanceInfos.ToArray();
        enhanceAbilityInfos = abilityInfos.ToArray();

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[EnhanceAbilitySlimeData] {key} 강화 데이터 LoadData 완료 ({commonSlimeEnhanceInfos.Length}레벨)");
    }

    // 강화 CSV가 없을 때 level 1~MAX_ENHANCE_LEVEL 공용 컬럼만 채운 임시 CSV를 만든다. (값은 전부 플레이스홀더)
    void CreateTempCsv(string path)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("level\tAttackPower\tAttackSpeed\tAttackRange\tEnhanceAbility\n");
        for (int lv = 1; lv <= MAX_ENHANCE_LEVEL; lv++)
            sb.Append($"{lv}\t0/0\t0/0\t0/0\t\n");

        System.IO.File.WriteAllText(path, sb.ToString(), System.Text.Encoding.UTF8);
        UnityEditor.AssetDatabase.Refresh();
        Debug.LogWarning($"[EnhanceAbilitySlimeData] CSV 파일 없어서 임시 생성: {path}");
    }

    // 강화 레벨에 상관없이 고정인 슬라임 고유 스탯(FlameDPS 등)을 uniqueSlimeStats에 설정한다. (한 번만 호출)
    protected void SetUniqueStat(SlimeStatType type, string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return;

        var stats = new List<SlimeStat>(uniqueSlimeStats ?? System.Array.Empty<SlimeStat>());
        int index = stats.FindIndex(s => s.statType == type);
        SlimeStat stat = new SlimeStat { statType = type, values = ParseValues(raw) };
        if (index >= 0) stats[index] = stat;
        else stats.Add(stat);
        uniqueSlimeStats = stats.ToArray();
    }

    // 드물게 슬라임 고유 스탯이 강화 레벨에 따라 달라지는 경우, 해당 레벨의 commonSlimeEnhanceInfos에 직접 덧붙인다.
    protected void AppendStat(int level, SlimeStatType type, string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return;
        SlimeEnhanceInfo info = GetCommonSlimeEnhanceInfo(level);
        if (info == null) return;

        var stats = new List<SlimeStat>(info.slimeStats)
        {
            new SlimeStat { statType = type, values = ParseValues(raw) }
        };
        info.slimeStats = stats.ToArray();
    }

    protected static string GetCol(string[] cols, int index)
        => index >= 0 && index < cols.Length ? cols[index].Trim() : "";

    protected static void AddStat(List<SlimeStat> stats, SlimeStatType type, string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return;
        stats.Add(new SlimeStat { statType = type, values = ParseValues(raw) });
    }

    // "5/7/10" 같은 슬래시 구분 값을 float[]로 변환한다.
    // Slime.cs가 values[mergeLevel](0~2)로 직접 인덱싱하므로 항상 MAX_LEVEL 칸을 채워서 반환한다.
    // 칸 수가 모자라면 마지막 값을 반복해서 채운다.
    protected static string[] ParseValues(string raw)
    {
        string[] parts = raw.Split('/');
        // float[] values = new float[SlimeData.MAX_LEVEL];
        // for (int i = 0; i < values.Length; i++)
        //     float.TryParse(parts[Mathf.Min(i, parts.Length - 1)].Trim(), out values[i]);
        return parts;
    }
#endif
}

[System.Serializable]
public class EnhanceAbilityInfo
{
    public int level;
    public string desc;
}
