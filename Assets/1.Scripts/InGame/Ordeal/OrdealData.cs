using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "OrdealData", menuName = "Data/OrdealData", order = 0)]
public class OrdealData : ScriptableObject
{
    public string key;
    public int level;
    public bool isHard;
    public OrdealType type;
    public string data;
    public string desc;
    public EnemyPatternType enemyPatternType;
    public EnemyPatternData enemyPatternData;

    public string MissionInfo()
    {
        switch (type)
        {
            case OrdealType.KillEnemies:
                return $"KILL {data} Enemies";
        }
        return "";
    }

#if UNITY_EDITOR
    public void LoadData()
    {
        string path = Path.Combine(Application.dataPath, "Json/OrdealData.csv");
        if (!File.Exists(path)) { Debug.LogWarning($"[OrdealData] CSV 없음: {path}"); return; }

        string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
        if (lines.Length < 2) return;

        string[] headers = lines[0].Split('\t');
        for (int i = 0; i < headers.Length; i++) headers[i] = headers[i].Trim();

        int iKey = System.Array.IndexOf(headers, "key");
        int iType = System.Array.IndexOf(headers, "type");
        int iDesc = System.Array.IndexOf(headers, "desc");
        int iLevel = System.Array.IndexOf(headers, "level");
        int iData = System.Array.IndexOf(headers, "data");
        int iIsHard = System.Array.IndexOf(headers, "isHard");
        int iEnemyPattern = System.Array.IndexOf(headers, "enemyPattern");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');
            if (Col(cols, iKey) != key) continue;

            if (System.Enum.TryParse(Col(cols, iType), out OrdealType t)) type = t;
            desc = Col(cols, iDesc);
            if (int.TryParse(Col(cols, iLevel), out int lv)) level = lv;
            data = Col(cols, iData);
            isHard = Col(cols, iIsHard).ToUpper() == "TRUE";

            if (System.Enum.TryParse(Col(cols, iEnemyPattern), out EnemyPatternType ep))
            {
                enemyPatternType = ep;
                enemyPatternData = Resources.Load<EnemyPatternData>($"EnemyPatternData/{enemyPatternType}");
            }
            break;
        }

        EditorUtility.SetDirty(this);
        Debug.Log($"[OrdealData] {key} 로드 완료");
    }

    static string Col(string[] cols, int idx) => idx >= 0 && idx < cols.Length ? cols[idx].Trim() : "";
#endif
}

public enum OrdealType
{
    KillEnemies,
    EndureTime,
    KillThemAll
}
