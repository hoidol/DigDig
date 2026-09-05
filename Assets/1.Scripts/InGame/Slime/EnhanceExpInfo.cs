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
        string path = System.IO.Path.Combine(Application.dataPath, "Json/EnhanceLevelInfo.csv");
        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning($"[EnhanceExpInfo] CSV 파일 없음: {path}");
            return;
        }

        string[] lines = System.IO.File.ReadAllLines(path, System.Text.Encoding.UTF8);
        if (lines.Length < 2) return;

        string[] headers = lines[0].Split('\t');
        int expCol = System.Array.IndexOf(headers, $"{grade}_Exp");
        int priceCol = System.Array.IndexOf(headers, $"{grade}_Price");
        if (expCol < 0 || priceCol < 0)
        {
            Debug.LogWarning($"[EnhanceExpInfo] CSV 헤더에서 '{grade}' 컬럼을 찾지 못함");
            return;
        }

        var expList = new System.Collections.Generic.List<int>();
        var priceList = new System.Collections.Generic.List<int>();
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');
            expList.Add(expCol < cols.Length && int.TryParse(cols[expCol].Trim(), out int exp) ? exp : 0);
            priceList.Add(priceCol < cols.Length && int.TryParse(cols[priceCol].Trim(), out int price) ? price : 0);
        }

        exps = expList.ToArray();
        prices = priceList.ToArray();
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[EnhanceExpInfo] {grade} LoadData 완료 (레벨 {exps.Length}개)");
    }
#endif
}
