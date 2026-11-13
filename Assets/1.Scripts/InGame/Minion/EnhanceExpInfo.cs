using UnityEngine;

[CreateAssetMenu]
public class EnhanceExpInfo : ScriptableObject
{
    public GradeType grade;
    public int[] exps;
    public int[] prices;//강화 가격

    public int TotalExp(int lv)
    {
        int sum = 0;
        for (int i = 0; i < exps.Length; i++)
        {
            if (i < lv)
            {
                sum += exps[i];
            }
            else
                break;
        }
        return sum;
    }

#if UNITY_EDITOR
    public void LoadData()
    {
        TableReader table = TableReader.Load("EnhanceLevelInfo.csv");
        if (table == null || table.rows.Count == 0) return;

        string expColumn = $"{grade}_Exp";
        string priceColumn = $"{grade}_Price";
        if (!table.HasColumn(expColumn) || !table.HasColumn(priceColumn))
        {
            Debug.LogWarning($"[EnhanceExpInfo] CSV 헤더에서 '{grade}' 컬럼을 찾지 못함");
            return;
        }

        var expList = new System.Collections.Generic.List<int>();
        var priceList = new System.Collections.Generic.List<int>();
        foreach (TableReader.Row row in table.rows)
        {
            expList.Add(row.TryGetInt(expColumn, out int exp) ? exp : 0);
            priceList.Add(row.TryGetInt(priceColumn, out int price) ? price : 0);
        }

        exps = expList.ToArray();
        prices = priceList.ToArray();
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[EnhanceExpInfo] {grade} LoadData 완료 (레벨 {exps.Length}개)");
    }
#endif
}
