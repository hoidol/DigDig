using UnityEngine;
#if UNITY_EDITOR
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
        TableReader table = TableReader.Load("EnemyData.csv");
        if (table == null) return;

        foreach (TableReader.Row row in table.rows)
        {
            if (!row.TryGetEnum("Type", out EnemyType rowType) || rowType != type) continue;
            if (row.TryGetFloat("AttackSpeed", out float v)) attackSpeed = v;
            if (row.TryGetFloat("AttackRange", out v)) attackRange = v;
            if (row.TryGetInt("Exp", out int n)) exp = n;
            if (row.TryGetFloat("HpMultiplier", out v)) hpMultiplier = v;
            if (row.TryGetFloat("AttackPowerMultiplier", out v)) attackPowerMultiplier = v;
            if (row.TryGetInt("Gold", out n)) gold = n;
            if (row.TryGetFloat("MoveSpeed", out v)) moveSpeed = v;

            EditorUtility.SetDirty(this);
            Debug.Log($"[EnemyData] {type} LoadData 완료");
            return;
        }
        Debug.LogWarning($"[EnemyData] CSV에서 type '{type}' 찾지 못함");
    }
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