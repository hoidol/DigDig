using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "EnemyPatternData", menuName = "EnemyPatternData", order = 0)]
public class EnemyPatternData : ScriptableObject
{
    public EnemyPatternType patternType;
    public EnemySpawnPatternData[] enemySpawnPatternDatas;
    public EnemyPattern enemyPatternPrefab;

#if UNITY_EDITOR
    public void LoadData()
    {
        string path = Path.Combine(Application.dataPath, "Json/EnemyPatternData.csv");
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[EnemyPatternData] CSV 파일 없음: {path}");
            return;
        }

        string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
        if (lines.Length < 2) return;

        string[] headers = lines[0].Split('\t');
        int iKey = System.Array.IndexOf(headers, "key");
        int iEnemyType = System.Array.IndexOf(headers, "enemyType");
        int iMinCount = System.Array.IndexOf(headers, "minCount");
        int iMaxCount = System.Array.IndexOf(headers, "maxCount");
        int iMinInterval = System.Array.IndexOf(headers, "minIntervalTime");
        int iMaxInterval = System.Array.IndexOf(headers, "maxIntervalTime");

        string patternKey = patternType.ToString().ToLower();
        var spawnList = new List<EnemySpawnPatternData>();

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');

            string rowKey = iKey >= 0 && iKey < cols.Length ? cols[iKey].Trim() : "";
            if (rowKey != patternKey) continue;

            var spawnData = new EnemySpawnPatternData();

            if (iEnemyType >= 0 && iEnemyType < cols.Length &&
                System.Enum.TryParse<EnemyType>(cols[iEnemyType].Trim(), out var enemyType))
                spawnData.enemyType = enemyType;

            int minCount = 0, maxCount = 0;
            if (iMinCount >= 0 && iMinCount < cols.Length) int.TryParse(cols[iMinCount].Trim(), out minCount);
            if (iMaxCount >= 0 && iMaxCount < cols.Length) int.TryParse(cols[iMaxCount].Trim(), out maxCount);
            spawnData.countRange = new Vector2Int(minCount, maxCount);

            float minInterval = 0f, maxInterval = 0f;
            if (iMinInterval >= 0 && iMinInterval < cols.Length) float.TryParse(cols[iMinInterval].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out minInterval);
            if (iMaxInterval >= 0 && iMaxInterval < cols.Length) float.TryParse(cols[iMaxInterval].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out maxInterval);
            spawnData.intervalRange = new Vector2(minInterval, maxInterval);

            spawnList.Add(spawnData);
        }

        enemySpawnPatternDatas = spawnList.ToArray();

        string prefabFolder = "Assets/3.Prefabs/Enemy/EnemyPattern";
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolder });
        foreach (string guid in guids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (go == null) continue;
            var pattern = go.GetComponent<EnemyPattern>();
            if (pattern != null && pattern.patternType == patternType)
            {
                enemyPatternPrefab = pattern;
                enemyPatternPrefab.enemyPatternData = this;
                Debug.Log($"[EnemyPatternData] {patternType} prefab 연결: {prefabPath}");
                break;
            }
        }

        EditorUtility.SetDirty(this);
        Debug.Log($"[EnemyPatternData] {patternType} LoadData 완료 (스폰 패턴 {enemySpawnPatternDatas.Length}개)");
    }
#endif
}

[System.Serializable]
public class EnemySpawnPatternData
{
    public EnemyType enemyType;
    public Vector2 intervalRange;
    public Vector2Int countRange;
}
