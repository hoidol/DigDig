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
    public int exp;
    public float hpMultiplier;
    public float attackPowerMultiplier;
    public int gold;
    public float moveSpeed = 1;

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
        int iExp = System.Array.IndexOf(headers, "exp");
        int iHpMul = System.Array.IndexOf(headers, "hpMultiplier");
        int iAtkMul = System.Array.IndexOf(headers, "attackPowerMultiplier");
        int iGold = System.Array.IndexOf(headers, "gold");
        int iMoveSpeed = System.Array.IndexOf(headers, "moveSpeed");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');

            if (!System.Enum.TryParse<EnemyType>(Col(cols, iType), out var rowType) || rowType != type) continue;
            if (float.TryParse(Col(cols, iAttackSpeed), NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) attackSpeed = v;
            if (float.TryParse(Col(cols, iAttackRange), NumberStyles.Float, CultureInfo.InvariantCulture, out v)) attackRange = v;
            if (int.TryParse(Col(cols, iExp), out var n)) exp = n;
            if (float.TryParse(Col(cols, iHpMul), NumberStyles.Float, CultureInfo.InvariantCulture, out v)) hpMultiplier = v;
            if (float.TryParse(Col(cols, iAtkMul), NumberStyles.Float, CultureInfo.InvariantCulture, out v)) attackPowerMultiplier = v;
            if (int.TryParse(Col(cols, iGold), out n)) gold = n;
            if (float.TryParse(Col(cols, iMoveSpeed), NumberStyles.Float, CultureInfo.InvariantCulture, out v)) moveSpeed = v;

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
    BonusElite,
    Boss
}