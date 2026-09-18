using UnityEngine;


[CreateAssetMenu(fileName ="Flame",menuName ="Growth1/Flame")]
public class FlameSlimeData : EnhanceAbilitySlimeData
{
    public float burnDuration = 4;

#if UNITY_EDITOR
    // 공용 컬럼은 base.LoadData()가 처리하고, FlameDPS(화염 고유 스탯)만 같은 csv에서 추가로 읽어
    // uniqueSlimeStats에 설정한다. FlameDPS는 강화 레벨에 상관없이 고정값이므로 한 번만 읽는다.
    public override void LoadData()
    {
        base.LoadData();

        string path = System.IO.Path.Combine(Application.dataPath, $"Json/SlimeEnhance/{key}.csv");
        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning($"[FlameSlimeData] CSV 파일 없음: {path}");
            return;
        }

        string[] lines = System.IO.File.ReadAllLines(path, System.Text.Encoding.UTF8);
        if (lines.Length < 2)
        {
            Debug.LogWarning($"[FlameSlimeData] CSV 라인 부족: {lines.Length}줄, path={path}");
            return;
        }

        string[] headers = ParseCsvLine(lines[0]);
        for (int i = 0; i < headers.Length; i++)
            headers[i] = headers[i].Trim();

        Debug.Log($"[FlameSlimeData] headers({headers.Length}): [{string.Join(" | ", headers)}]");

        int iFlameDPS = System.Array.IndexOf(headers, "FlameDPS");
        if (iFlameDPS < 0)
        {
            Debug.LogWarning($"[FlameSlimeData] 'FlameDPS' 컬럼을 찾지 못함. headers=[{string.Join(" | ", headers)}]");
            return;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = ParseCsvLine(lines[i]);

            string raw = GetCol(cols, iFlameDPS);
            if (string.IsNullOrWhiteSpace(raw)) continue;

            SetUniqueStat(SlimeStatType.FlameDPS, raw);
            break;
        }

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[FlameSlimeData] {key} FlameDPS 로드 완료");
    }
#endif
}
