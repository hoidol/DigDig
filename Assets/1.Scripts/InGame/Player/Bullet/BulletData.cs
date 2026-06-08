using UnityEngine;
#if UNITY_EDITOR
using System.Globalization;
using System.IO;
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Data/BulletData", fileName = "BulletData")]
public class BulletData : ScriptableObject
{
    public static readonly int MAX_LEVEL = 3;
    public string key;
    public string Title => itemName;
    public string itemName;

    public bool specialBullet;// 일반을 제외하고 모두 특수 
    public bool mergedBullet; // 머지 횟수 
    public string desc;

    public Sprite thumbnail;
    public Grade grade;      // 등급별 필터링용
    public ConditionData[] addEffectUnlockConditions; // 추가 효과 해금 조건 (모두 충족해야 효과 활성화)
    public int applyOrder; // 아이템 적용 순서
    public string[] mergeKeys; // 해당 총알과 머지가 가능한 총알키 
    public float multiplyATK; // 공격력 배율 (예: 1.2f는 20% 증가)
    public PlayerBulletObject prefab;

    public static Color GetGradeColor(Grade grade)
    {
        return grade switch
        {
            Grade.Normal => new Color(0.78f, 0.78f, 0.78f), // 회색
            Grade.Rare => new Color(0.00f, 0.44f, 0.87f), // 파랑
            Grade.Unique => new Color(0.56f, 0.28f, 0.98f), // 보라
            Grade.Legend => new Color(1.00f, 0.64f, 0.00f), // 주황/금
            Grade.Myth => new Color(0.98f, 0.18f, 0.18f), // 빨강
            _ => Color.white
        };
    }

    public string GetDescription(bool detail = false)
    {
        return BulletManager.Create(key).GetDescription(detail);
    }

    public static BulletData GetBulletData(string key)
    {
        return BulletManager.Instance.GetBulletData(key);
    }

#if UNITY_EDITOR
    public void LoadData()
    {
        string path = Path.Combine(Application.dataPath, "Json/BulletData.csv");
        if (!File.Exists(path)) { Debug.LogWarning($"[BulletData] CSV 없음: {path}"); return; }

        string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
        if (lines.Length < 2) return;

        string[] headers = lines[0].Split('\t');
        for (int i = 0; i < headers.Length; i++) headers[i] = headers[i].Trim();

        int iKey = System.Array.IndexOf(headers, "key");
        int iName = System.Array.IndexOf(headers, "Name");
        int iSpecial = System.Array.IndexOf(headers, "specialBullet");
        int iDesc = System.Array.IndexOf(headers, "Desc");
        int iMultiplyATK = System.Array.IndexOf(headers, "multiplyATK");
        int iMergeKeys = System.Array.IndexOf(headers, "mergeKeys");
        int iCondTypes = System.Array.IndexOf(headers, "conditionTypes");
        int iCondValues = System.Array.IndexOf(headers, "conditionValues");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');

            if (Col(cols, iKey) != key) continue;

            itemName = Col(cols, iName);
            desc = Col(cols, iDesc);
            specialBullet = Col(cols, iSpecial).ToUpper() == "TRUE";

            if (float.TryParse(Col(cols, iMultiplyATK), NumberStyles.Float, CultureInfo.InvariantCulture, out float atk)) multiplyATK = atk;

            string mergeRaw = Col(cols, iMergeKeys);
            mergeKeys = string.IsNullOrEmpty(mergeRaw) ? new string[0] : mergeRaw.Split('/');

            string condTypesRaw = Col(cols, iCondTypes);
            string condValuesRaw = Col(cols, iCondValues);
            if (!string.IsNullOrEmpty(condTypesRaw))
            {
                string[] types = condTypesRaw.Split('/');
                string[] values = string.IsNullOrEmpty(condValuesRaw) ? new string[0] : condValuesRaw.Split('/');
                var condList = new System.Collections.Generic.List<ConditionData>();
                for (int j = 0; j < types.Length; j++)
                {
                    if (!System.Enum.TryParse<ConditionType>(types[j].Trim(), out var ct)) continue;
                    var cond = new ConditionData { conditionType = ct };
                    if (j < values.Length && int.TryParse(values[j].Trim(), out int cv)) cond.count = cv;
                    condList.Add(cond);
                }
                addEffectUnlockConditions = condList.ToArray();
            }
            else
            {
                addEffectUnlockConditions = new ConditionData[0];
            }

            EditorUtility.SetDirty(this);
            Debug.Log($"[BulletData] {key} LoadData 완료");
            return;
        }

        Debug.LogWarning($"[BulletData] CSV에서 key '{key}' 를 찾지 못함");
    }

    static string Col(string[] cols, int idx) => idx >= 0 && idx < cols.Length ? cols[idx].Trim() : "";
#endif
}