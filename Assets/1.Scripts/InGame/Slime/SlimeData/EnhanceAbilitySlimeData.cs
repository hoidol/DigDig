using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public abstract class EnhanceAbilitySlimeData : SlimeData
{
    public EnhanceAbilityInfo[] enhanceAbilityInfos;

    // commonSlimeEnhanceInfos: 강화 레벨(1~15)마다 달라지는 공용 스탯(AttackPower. AttackSpeed·AttackRange·CritChance는 SlimeData.csv의 baseSlimeStats) + EnhanceAbility 설명
    // uniqueSlimeStats: 강화 레벨에 상관없이 고정인 슬라임 고유 스탯 (mergeLv별 값). CSV가 아니라 각 슬라임 SlimeData의 LoadData 재정의에서 SetUniqueStat으로 입력
    public List<SlimeStat> uniqueSlimeStats =new List<SlimeStat>();
    public SlimeStat GetUniqueStat(StatType statType)
    {
        return uniqueSlimeStats.FirstOrDefault(e => e.statType == statType);
    }

    public override SlimeStat GetSlimeStat(StatType statType, int enhanceLv)
    {
        SlimeStat slimeStat = base.GetSlimeStat(statType,enhanceLv);
        if(slimeStat == null)
        {
            return GetUniqueStat(statType);
        }

        return slimeStat;
    }

#if UNITY_EDITOR
    // 강화 데이터는 등급별 표(SlimeEnhance/SlimeEnhance_{등급}.csv, 시트 탭도 등급별).
    // 읽을 때는 등급과 상관없이 SlimeEnhance_*.csv 전체에서 내 key 블록을 찾는다 → 등급이 바뀌어도 행만 옮기면 됨
    // 표 안에서 Key는 블록 첫 행에만 쓰고 아래 행은 비워 두면 위 Key를 이어받는다
    public const string SLIME_ENHANCE_FOLDER = "SlimeEnhance";
    public const string SLIME_ENHANCE_FILE_PATTERN = "SlimeEnhance_*.csv";

    // 내 key 블록의 Level, AttackPower, EnhanceAbility 컬럼만 읽어 (AttackSpeed 열이 있어도 무시)
    // commonSlimeEnhanceInfos, enhanceAbilityInfos 배열을 채운다. (그 외 컬럼은 무시)
    // 슬라임별 고유 스탯은 자식 클래스에서 LoadData를 재정의해 base.LoadData() 뒤에 SetUniqueStat으로 넣는다.
    public override void LoadData()
    {
        base.LoadData();
        uniqueSlimeStats.Clear(); // 자식 LoadData에서 다시 채움

        List<TableReader.Row> myRows = null;
        string foundFile = null;
        string folder = TableReader.GetFullPath(SLIME_ENHANCE_FOLDER);
        string[] files = System.IO.Directory.Exists(folder)
            ? System.IO.Directory.GetFiles(folder, SLIME_ENHANCE_FILE_PATTERN)
            : new string[0];
        foreach (string file in files.OrderBy(f => f))
        {
            string relativePath = $"{SLIME_ENHANCE_FOLDER}/{System.IO.Path.GetFileName(file)}";
            List<TableReader.Row> rows = FindKeyRows(TableReader.Load(relativePath));
            if (rows.Count == 0)
                continue;
            if (myRows != null)
            {
                Debug.LogWarning($"[EnhanceAbilitySlimeData] '{key}' 블록이 {foundFile}, {relativePath} 두 곳에 있음 → {foundFile} 사용");
                continue;
            }
            myRows = rows;
            foundFile = relativePath;
        }

        if (myRows == null)
        {
            // 기존 데이터를 지우지 않음 (표에 아직 안 넣은 슬라임)
            Debug.LogWarning($"[EnhanceAbilitySlimeData] {SLIME_ENHANCE_FOLDER}/{SLIME_ENHANCE_FILE_PATTERN}에 '{key}' 블록 없음");
            return;
        }

        var enhanceInfos = new List<SlimeEnhanceInfo>();
        var abilityInfos = new List<EnhanceAbilityInfo>();

        foreach (TableReader.Row row in myRows)
        {
            if (!row.TryGetInt("Level", out int level))
                continue;

            var stats = new List<SlimeStat>();
            AddStat(stats, StatType.AttackPower, row.Get("AttackPower"));
            enhanceInfos.Add(new SlimeEnhanceInfo { level = level, slimeStats = stats.ToArray() });

            string desc = row.Get("EnhanceAbility");
            if (!string.IsNullOrWhiteSpace(desc))
                abilityInfos.Add(new EnhanceAbilityInfo { level = level, desc = desc });
        }

        commonSlimeEnhanceInfos = enhanceInfos.ToArray();
        enhanceAbilityInfos = abilityInfos.ToArray();

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[EnhanceAbilitySlimeData] {key} 강화 데이터 LoadData 완료 ({foundFile}, {commonSlimeEnhanceInfos.Length}레벨)");
    }

    // 표에서 내 key 블록의 행만 (Key가 빈 행은 위 Key를 이어받음)
    List<TableReader.Row> FindKeyRows(TableReader table)
    {
        var result = new List<TableReader.Row>();
        if (table == null)
            return result;

        string curKey = null;
        foreach (TableReader.Row row in table.rows)
        {
            string rowKey = row.Get("Key");
            if (!string.IsNullOrEmpty(rowKey))
                curKey = rowKey;
            if (curKey == key)
                result.Add(row);
        }
        return result;
    }

    // 강화 레벨에 상관없이 고정인 슬라임 고유 스탯을 uniqueSlimeStats에 설정한다. 같은 type이 있으면 덮어씀
    // raw는 CSV와 같은 "1/2/3" 형식 (mergeLv별 값, 2개면 기본/증가량)
    protected void SetUniqueStat(StatType type, string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return;

        SlimeStat stat = new SlimeStat { statType = type, values = ParseValues(raw) };
        int index = uniqueSlimeStats.FindIndex(s => s.statType == type);
        if (index >= 0) uniqueSlimeStats[index] = stat;
        else uniqueSlimeStats.Add(stat);
    }

    // 드물게 슬라임 고유 스탯이 강화 레벨에 따라 달라지는 경우, 해당 레벨의 commonSlimeEnhanceInfos에 직접 덧붙인다.
    protected void AppendStat(int level, StatType type, string raw)
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

    protected static void AddStat(List<SlimeStat> stats, StatType type, string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return;
        stats.Add(new SlimeStat { statType = type, values = ParseValues(raw) });
    }

    // "5/7/10" 같은 슬래시 구분 값을 float[]로 변환한다.
    // 값 개수 규칙은 SlimeStat.GetValue 참고 (2개=기본/증가량, 그 외=mergeLevel별, 모자라면 마지막 값)
    protected static string[] ParseValues(string raw)
    {
        string[] parts = raw.Split('/');
        // float[] values = new float[SlimeData.MAX_MERGE_LEVEL + 1];
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
