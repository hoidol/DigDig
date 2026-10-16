using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "EnemyData", menuName = "EnemyData", order = 0)]
public class EnemyData : ScriptableObject
{
    public EnemyType type;
    public DefenceType defenceType;
    public int exp;
    public float hpMultiplier;
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
            if (row.TryGetInt("Exp", out int n)) exp = n;
            if (row.TryGetFloat("HpMultiplier", out float v)) hpMultiplier = v;
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
    Normal,
    Elite,
    BonusElite,
    Boss,        
}

//갑옷(바위), 중갑(수정), 정령(진흙)
public enum DefenceType
{
    Normal, //일반형 - 직관적으로 데미지 입음
    Armored, //갑옷형 - 물리 -50% 마법+50% 진동0%
    Ethereal, //정령형 - 물리 +50% 마법-50% 진동0%
    HeavyArmor, // 중갑형 - 진동 +60% 물리 -30% 마법 -30%
    Organic, //생체형 - 물리 +30% 마법 -30% 진동0%, 버프 무시
}