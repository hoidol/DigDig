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
    // public int gold;
    public float moveSpeed = 1;

#if UNITY_EDITOR
    public void LoadData()
    {
        TableReader table = TableReader.Load("EnemyData.csv");
        if (table == null) return;

        // 같은 Type이라도 방어 타입별로 행이 나뉘므로 DefenceType 컬럼이 있으면 같이 맞춘다
        bool hasDefenceColumn = table.HasColumn("DefenceType");
        foreach (TableReader.Row row in table.rows)
        {
            if (!row.TryGetEnum("Type", out EnemyType rowType) || rowType != type) continue;
            if (hasDefenceColumn && (!row.TryGetEnum("DefenceType", out DefenceType rowDefence) || rowDefence != defenceType)) continue;
            if (row.TryGetInt("Exp", out int n)) exp = n;
            if (row.TryGetFloat("HpMultiplier", out float v)) hpMultiplier = v;
            // if (row.TryGetInt("Gold", out n)) gold = n;
            if (row.TryGetFloat("MoveSpeed", out v)) moveSpeed = v;

            EditorUtility.SetDirty(this);
            Debug.Log($"[EnemyData] {name} ({type}/{defenceType}) LoadData 완료");
            return;
        }
        Debug.LogWarning($"[EnemyData] {name}: CSV에서 '{type}/{defenceType}' 행 찾지 못함");
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
    // (2026-11-21) 방어 타입 4종 = 경갑·중갑·중장갑·영체. 코드 이름은 그대로 두고 화면 이름만 바꿈 (CSV·시트·프리팹이 이름/번호로 저장돼 있어서)
    // 배율은 Resources/DefenceInfo 표 한 곳
    Normal = 0,   // 중갑 (Medium). 엘리트·보스도 이 타입
    Armored = 1,  // 중장갑 (Heavy)
    Ethereal = 2, // 영체 (Spirit)
    // 3: 이전 중갑형(HeavyArmor) 삭제, 4: 생체형(Organic) 삭제 → 기존 직렬화 값 유지를 위해 번호 비워둠
    Light = 5,    // 경갑 (Light) (2026-11-21 추가)
}