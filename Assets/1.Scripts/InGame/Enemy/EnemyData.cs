using UnityEngine;
#if UNITY_EDITOR
using System.Globalization;
using System.IO;
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "EnemyData", menuName = "EnemyData", order = 0)]
public class EnemyData : ScriptableObject
{
    public EnemyType type;
    public float attackSpeed;
    public float attackRange;
    public float moveRange;
    public int exp;
    public Enemy prefab;
    public Vector2Int size;
    public bool breakTileWhenSpawn;

    public float hpMultiplier;
    public float attackPowerMultiplier;

    // 최종 체력 = stageData.enemyHp * 에너미 배율 * 층 배율 * 웨이브 배율
    public float GetHp()
    {
        return GameManager.Instance.stageData.GetPhaseData().enemyHp * hpMultiplier;
    }

    // 최종 공격력 = stageData.enemyAttackPower * 에너미 배율 * 층 배율 * 웨이브 배율
    public float GetAttackPower()
    {
        return GameManager.Instance.stageData.GetPhaseData().enemyAttackPower * attackPowerMultiplier;
    }

#if UNITY_EDITOR
    public void LoadData()
    {
        string path = Path.Combine(Application.dataPath, "Json/EnemyData.csv");
        if (!File.Exists(path)) { Debug.LogWarning($"[EnemyData] CSV 없음: {path}"); return; }

        string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
        if (lines.Length < 2) return;

        string[] headers = lines[0].Split('\t');
        int iType = System.Array.IndexOf(headers, "type");
        int iAttackSpeed = System.Array.IndexOf(headers, "attackSpeed");
        int iAttackRange = System.Array.IndexOf(headers, "attackRange");
        int iMoveRange = System.Array.IndexOf(headers, "moveRange");
        int iExp = System.Array.IndexOf(headers, "exp");
        int iHpMul = System.Array.IndexOf(headers, "hpMultiplier");
        int iAtkMul = System.Array.IndexOf(headers, "attackPowerMultiplier");
        int iSize = System.Array.IndexOf(headers, "size");
        int iBreakTile = System.Array.IndexOf(headers, "breakTileWhenSpawn");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');

            if (!System.Enum.TryParse<EnemyType>(Col(cols, iType), out var rowType) || rowType != type) continue;
            if (float.TryParse(Col(cols, iAttackSpeed), NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) attackSpeed = v;
            if (float.TryParse(Col(cols, iAttackRange), NumberStyles.Float, CultureInfo.InvariantCulture, out v)) attackRange = v;
            if (float.TryParse(Col(cols, iMoveRange), NumberStyles.Float, CultureInfo.InvariantCulture, out v)) moveRange = v;
            if (int.TryParse(Col(cols, iExp), out var e)) exp = e;
            if (float.TryParse(Col(cols, iHpMul), NumberStyles.Float, CultureInfo.InvariantCulture, out v)) hpMultiplier = v;
            if (float.TryParse(Col(cols, iAtkMul), NumberStyles.Float, CultureInfo.InvariantCulture, out v)) attackPowerMultiplier = v;
            if (int.TryParse(Col(cols, iSize), out var s)) size = new Vector2Int(s, s);
            var breakStr = Col(cols, iBreakTile).ToUpper();
            if (breakStr == "TRUE" || breakStr == "FALSE") breakTileWhenSpawn = breakStr == "TRUE";

            EditorUtility.SetDirty(this);
            Debug.Log($"[EnemyData] {type} LoadData 완료");
            return;
        }
        Debug.LogWarning($"[EnemyData] CSV에서 type '{type}' 찾지 못함");
    }

    static string Col(string[] cols, int idx) => idx >= 0 && idx < cols.Length ? cols[idx].Trim() : "";
#endif
}

public enum EnemyType
{
    Melee,
    Ranged,
    Elite,
    Boss
}