using UnityEngine;

[CreateAssetMenu]
public class EnhanceGradeInfo : ScriptableObject
{
    public GradeType grade;
    public int baseEnhance;

#if UNITY_EDITOR
    public void LoadData()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "Json/EnhanceGradeInfo.csv");
        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning($"[EnhanceGradeInfo] CSV 파일 없음: {path}");
            return;
        }

        string[] lines = System.IO.File.ReadAllLines(path, System.Text.Encoding.UTF8);
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');
            if (cols.Length < 2) continue;
            if (!System.Enum.TryParse(cols[0].Trim(), out GradeType rowGrade) || rowGrade != grade) continue;
            if (!int.TryParse(cols[1].Trim(), out int value)) continue;

            baseEnhance = value;
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[EnhanceGradeInfo] {grade} LoadData 완료");
            return;
        }

        Debug.LogWarning($"[EnhanceGradeInfo] CSV에서 '{grade}' 를 찾지 못함");
    }
#endif
}
