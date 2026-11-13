using System.Collections.Generic;
using System.Linq;
using UnityEngine;



// 강화 레벨(로비 영구 성장, 0~14)별 스탯 데이터를 가진 슬라임 데이터. 강화 레벨은 스탯만 올리고 특수 능력은 없다
public abstract class EnhanceMinionData : MinionData
{
    // commonMinionEnhanceInfos: 강화 레벨마다 달라지는 공용 스탯(AttackPower. AttackSpeed·AttackRange·CritChance는 MinionData.csv의 baseMinionStats)
    // uniqueMinionStats: 강화 레벨에 상관없이 고정인 슬라임 고유 스탯 (mergeLv별 값). CSV가 아니라 각 슬라임 MinionData의 LoadData 재정의에서 SetUniqueStat으로 입력
    public List<MinionStat> uniqueMinionStats =new List<MinionStat>();
    public MinionStat GetUniqueStat(StatType statType)
    {
        return uniqueMinionStats.FirstOrDefault(e => e.statType == statType);
    }

    public override MinionStat GetMinionStat(StatType statType, int enhanceLv)
    {
        MinionStat minionStat = base.GetMinionStat(statType,enhanceLv);
        if(minionStat == null)
        {
            return GetUniqueStat(statType);
        }

        return minionStat;
    }

#if UNITY_EDITOR
    // 강화 데이터는 등급별 표(MinionEnhance/MinionEnhance_{등급}.csv, 시트 탭도 등급별).
    // 읽을 때는 등급과 상관없이 MinionEnhance_*.csv 전체에서 내 key 블록을 찾는다 → 등급이 바뀌어도 행만 옮기면 됨
    // 표 안에서 Key는 블록 첫 행에만 쓰고 아래 행은 비워 두면 위 Key를 이어받는다
    public const string MINION_ENHANCE_FOLDER = "MinionEnhance";
    public const string MINION_ENHANCE_FILE_PATTERN = "MinionEnhance_*.csv";

    // 내 key 블록의 Level, AttackPower 컬럼만 읽어 commonMinionEnhanceInfos를 채운다. (그 외 컬럼은 무시)
    // 슬라임별 고유 스탯은 자식 클래스에서 LoadData를 재정의해 base.LoadData() 뒤에 SetUniqueStat으로 넣는다.
    public override void LoadData()
    {
        base.LoadData();
        uniqueMinionStats.Clear(); // 자식 LoadData에서 다시 채움

        List<TableReader.Row> myRows = null;
        string foundFile = null;
        string folder = TableReader.GetFullPath(MINION_ENHANCE_FOLDER);
        string[] files = System.IO.Directory.Exists(folder)
            ? System.IO.Directory.GetFiles(folder, MINION_ENHANCE_FILE_PATTERN)
            : new string[0];
        foreach (string file in files.OrderBy(f => f))
        {
            string relativePath = $"{MINION_ENHANCE_FOLDER}/{System.IO.Path.GetFileName(file)}";
            List<TableReader.Row> rows = FindKeyRows(TableReader.Load(relativePath));
            if (rows.Count == 0)
                continue;
            if (myRows != null)
            {
                Debug.LogWarning($"[EnhanceMinionData] '{key}' 블록이 {foundFile}, {relativePath} 두 곳에 있음 → {foundFile} 사용");
                continue;
            }
            myRows = rows;
            foundFile = relativePath;
        }

        if (myRows == null)
        {
            // 기존 데이터를 지우지 않음 (표에 아직 안 넣은 슬라임)
            Debug.LogWarning($"[EnhanceMinionData] {MINION_ENHANCE_FOLDER}/{MINION_ENHANCE_FILE_PATTERN}에 '{key}' 블록 없음");
            return;
        }

        var enhanceInfos = new List<MinionEnhanceInfo>();

        foreach (TableReader.Row row in myRows)
        {
            if (!row.TryGetInt("Level", out int level))
                continue;

            var stats = new List<MinionStat>();
            AddStat(stats, StatType.AttackPower, row.Get("AttackPower"));
            enhanceInfos.Add(new MinionEnhanceInfo { level = level, minionStats = stats.ToArray() });
        }

        commonMinionEnhanceInfos = enhanceInfos.ToArray();

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[EnhanceMinionData] {key} 강화 데이터 LoadData 완료 ({foundFile}, {commonMinionEnhanceInfos.Length}레벨)");
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

    // 강화 레벨에 상관없이 고정인 슬라임 고유 스탯을 uniqueMinionStats에 설정한다. 같은 type이 있으면 덮어씀
    // raw는 CSV와 같은 "1/2/3" 형식 (mergeLv별 값, 2개면 기본/증가량)
    protected void SetUniqueStat(StatType type, string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return;

        MinionStat stat = new MinionStat { statType = type, values = ParseValues(raw) };
        int index = uniqueMinionStats.FindIndex(s => s.statType == type);
        if (index >= 0) uniqueMinionStats[index] = stat;
        else uniqueMinionStats.Add(stat);
    }

    // 드물게 슬라임 고유 스탯이 강화 레벨에 따라 달라지는 경우, 해당 레벨의 commonMinionEnhanceInfos에 직접 덧붙인다.
    protected void AppendStat(int level, StatType type, string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return;
        MinionEnhanceInfo info = GetCommonMinionEnhanceInfo(level);
        if (info == null) return;

        var stats = new List<MinionStat>(info.minionStats)
        {
            new MinionStat { statType = type, values = ParseValues(raw) }
        };
        info.minionStats = stats.ToArray();
    }

    protected static void AddStat(List<MinionStat> stats, StatType type, string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return;
        stats.Add(new MinionStat { statType = type, values = ParseValues(raw) });
    }

    // "5/7/10" 같은 슬래시 구분 값을 float[]로 변환한다.
    // 값 개수 규칙은 MinionStat.GetValue 참고 (2개=기본/증가량, 그 외=mergeLevel별, 모자라면 마지막 값)
    protected static string[] ParseValues(string raw)
    {
        string[] parts = raw.Split('/');
        // float[] values = new float[MinionData.MAX_MERGE_LEVEL + 1];
        // for (int i = 0; i < values.Length; i++)
        //     float.TryParse(parts[Mathf.Min(i, parts.Length - 1)].Trim(), out values[i]);
        return parts;
    }
#endif
}

