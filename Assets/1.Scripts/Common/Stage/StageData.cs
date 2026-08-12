using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "StageData", menuName = "StageData")]
public class StageData : ScriptableObject
{
    public EnemySpawnerContainer enemySpawnerContainerPrefab;

    // public BossSpawner bossSpawner;
    public string key;
    public int order;
    public string Title => key;
    public int level; //Mode

    public float oreHp;
    public Enemy[] enemyPrefabs; //해당 스테이지의 등장할 원거리, 근거리 엘리트 등 적 설정
    Dictionary<EnemyType, Enemy> enemyPrefabDic = new Dictionary<EnemyType, Enemy>();
    public PhaseData[] phaseDatas;
    public EventData[] eventDatas;
    public Boss boss;
    public PhaseData GetPhaseData(int idx = -1)
    {
        if (idx < 0)
            idx = GameManager.Instance.day;
        return phaseDatas[idx];
    }
    public void Init()
    {
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            enemyPrefabDic.Add(enemyPrefabs[i].enemyType, enemyPrefabs[i]);
        }
    }

    public Enemy GetEnemyPrefab(EnemyType enemyType)
    {
        return enemyPrefabDic[enemyType];
    }

    public static StageData GetStageData(string key)
    {
        if (StageManager.Instance != null)
        {
            return StageManager.Instance.GetStageData(key);
        }
        return Resources.Load<StageData>($"StageData/{key}");
    }

#if UNITY_EDITOR
    public void LoadData()
    {
        LoadStageBasic();
        // LoadUndergroundsAndWaves();
        EditorUtility.SetDirty(this);
        // Debug.Log($"[StageData] {key} LoadData 완료 ({undergroundDatas.Length}개 층)");
    }

    void LoadStageBasic()
    {
        string path = Path.Combine(Application.dataPath, "Json/StageData.csv");
        if (!File.Exists(path)) { Debug.LogWarning($"[StageData] CSV 없음: {path}"); return; }

        string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
        if (lines.Length < 2) return;

        string[] headers = lines[0].Split('\t');
        int iKey = System.Array.IndexOf(headers, "key");
        int iLevel = System.Array.IndexOf(headers, "level");
        int iOreHp = System.Array.IndexOf(headers, "oreHp");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');
            if (Col(cols, iKey) != key) continue;

            if (int.TryParse(Col(cols, iLevel), out var lv)) level = lv;
            if (float.TryParse(Col(cols, iOreHp), NumberStyles.Float, CultureInfo.InvariantCulture, out float oh)) oreHp = oh;
            break;
        }

        LoadPhaseDatas();
        LoadEventDatas();
        LoadEnemyPrefabs();
        LoadBossPrefab();
    }

    void LoadBossPrefab()
    {
        string folderPath = $"Assets/3.Prefabs/{key}/Boss";
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        foreach (string guid in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            var b = go != null ? go.GetComponent<Boss>() : null;
            if (b != null) { boss = b; break; }
        }
        Debug.Log($"[StageData] {key} BossPrefab 로드 완료: {(boss != null ? boss.name : "없음")}");
    }

    void LoadEnemyPrefabs()
    {
        string folderPath = $"Assets/3.Prefabs/{key}/Enemy";
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        var list = new List<Enemy>();
        foreach (string guid in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            var e = go != null ? go.GetComponent<Enemy>() : null;
            if (e != null) list.Add(e);
        }
        enemyPrefabs = list.ToArray();
        Debug.Log($"[StageData] {key} EnemyPrefabs {list.Count}개 로드 완료");
    }

    void LoadPhaseDatas()
    {
        string path = Path.Combine(Application.dataPath, "Json/PhaseData.csv");
        if (!File.Exists(path)) { Debug.LogWarning($"[StageData] PhaseData CSV 없음: {path}"); return; }

        string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
        if (lines.Length < 2) return;

        string[] headers = lines[0].Split('\t');
        for (int i = 0; i < headers.Length; i++) headers[i] = headers[i].Trim();

        int phase = System.Array.IndexOf(headers, "phase");
        int iIsBoss = System.Array.IndexOf(headers, "isBoss");
        int iOrdealLevels = System.Array.IndexOf(headers, "ordealLevels");
        int iEnemyHp = System.Array.IndexOf(headers, "enemyHp");
        int iEnemyAtk = System.Array.IndexOf(headers, "enemyAttackPower");

        var list = new List<PhaseData>();
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');

            var d = new PhaseData();
            if (int.TryParse(Col(cols, phase), out int p)) d.phase = p;
            d.isBoss = Col(cols, iIsBoss).ToUpper() == "TRUE";
            if (int.TryParse(Col(cols, iOrdealLevels), out int ol)) d.ordealLevel = ol;
            if (float.TryParse(Col(cols, iEnemyHp), NumberStyles.Float, CultureInfo.InvariantCulture, out float eh)) d.enemyHp = eh;
            if (float.TryParse(Col(cols, iEnemyAtk), NumberStyles.Float, CultureInfo.InvariantCulture, out float ea)) d.enemyAttackPower = ea;

            d.enemyPatternData = FindEnemyPatternData(d.phase);

            list.Add(d);
        }

        phaseDatas = list.ToArray();
        Debug.Log($"[StageData] {key} PhaseData {list.Count}개 로드 완료");
    }

    EnemyPatternData FindEnemyPatternData(int phase)
    {
        // Debug.Log($"FindEnemyPatternData {phase}");
        string path = Path.Combine(Application.dataPath, "Json/EnemyPatternData.csv");
        if (!File.Exists(path)) { Debug.LogWarning($"[StageData] EnemyPatternData CSV 없음: {path}"); return null; }

        string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
        if (lines.Length < 2)
        {
            Debug.Log("FindEnemyPatternData if (lines.Length < 2)");
            return null;
        }

        string[] headers = lines[0].Split('\t');
        for (int i = 0; i < headers.Length; i++) headers[i] = headers[i].Trim();

        int iStage = System.Array.IndexOf(headers, "stage");
        int iPhase = System.Array.IndexOf(headers, "phase");

        int iTrigger = System.Array.IndexOf(headers, "triggerTime");
        int iEnd = System.Array.IndexOf(headers, "EndTime");
        int iEnemy = System.Array.IndexOf(headers, "enemyType");
        int iMinCount = System.Array.IndexOf(headers, "minCount");
        int iMaxCount = System.Array.IndexOf(headers, "maxCount");
        int iMinItvl = System.Array.IndexOf(headers, "minIntervalTime");
        int iMaxItvl = System.Array.IndexOf(headers, "maxIntervalTime");

        var list = new List<EnemySpawnPatternData>();
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');
            //if (Col(cols, iStage) != key) continue;
            if (!int.TryParse(Col(cols, iPhase), out int ph) || ph != phase) continue;

            var e = new EnemySpawnPatternData { phase = ph };
            // if (float.TryParse(Col(cols, iTrigger), NumberStyles.Float, CultureInfo.InvariantCulture, out float tr)) e.triggerTime = tr;
            // if (float.TryParse(Col(cols, iEnd), NumberStyles.Float, CultureInfo.InvariantCulture, out float en)) e.endTime = en;
            if (System.Enum.TryParse(Col(cols, iEnemy), out EnemyType et)) e.enemyType = et;
            if (int.TryParse(Col(cols, iMinCount), out int minC) && int.TryParse(Col(cols, iMaxCount), out int maxC))
                e.countRange = new Vector2Int(minC, maxC);
            if (float.TryParse(Col(cols, iMinItvl), NumberStyles.Float, CultureInfo.InvariantCulture, out float minI) &&
                float.TryParse(Col(cols, iMaxItvl), NumberStyles.Float, CultureInfo.InvariantCulture, out float maxI))
                e.intervalRange = new Vector2(minI, maxI);
            list.Add(e);
        }

        if (list.Count == 0) { Debug.LogWarning($"[StageData] EnemyPatternData stage={key} phase={phase} 데이터 없음"); return null; }

        return new EnemyPatternData { enemySpawnPatternDatas = list.ToArray() };
    }

    void LoadEventDatas()
    {
        string path = Path.Combine(Application.dataPath, "Json/EventData.csv");
        if (!File.Exists(path)) { Debug.LogWarning($"[StageData] EventData CSV 없음: {path}"); return; }

        string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
        if (lines.Length < 2) return;

        string[] headers = lines[0].Split('\t');
        for (int i = 0; i < headers.Length; i++) headers[i] = headers[i].Trim();

        int iTypes = System.Array.IndexOf(headers, "eventTypes");
        int iChances = System.Array.IndexOf(headers, "chances");
        int iTriggers = System.Array.IndexOf(headers, "triggers");
        int iPhaseIdx = System.Array.IndexOf(headers, "phaseIdx");

        var list = new List<EventData>();
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');

            var d = new EventData();

            string typesRaw = Col(cols, iTypes);
            if (!string.IsNullOrEmpty(typesRaw))
            {
                var typeList = new List<EventType>();
                foreach (var t in typesRaw.Split('/'))
                    if (System.Enum.TryParse(t.Trim(), out EventType et)) typeList.Add(et);
                d.eventTypes = typeList.ToArray();
            }

            string chancesRaw = Col(cols, iChances);
            if (!string.IsNullOrEmpty(chancesRaw))
            {
                var chanceList = new List<float>();
                foreach (var c in chancesRaw.Split('/'))
                    if (float.TryParse(c.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float cv)) chanceList.Add(cv);
                d.chances = chanceList.ToArray();
            }

            string triggersRaw = Col(cols, iTriggers);
            if (!string.IsNullOrEmpty(triggersRaw))
            {
                var trigList = new List<EventTrigger>();
                foreach (var t in triggersRaw.Split('/'))
                    if (System.Enum.TryParse(t.Trim(), out EventTrigger et)) trigList.Add(et);
                d.triggers = trigList.ToArray();
            }

            if (int.TryParse(Col(cols, iPhaseIdx), out int pi)) d.phaseIdx = pi;

            list.Add(d);
        }

        eventDatas = list.ToArray();
        Debug.Log($"[StageData] {key} EventData {list.Count}개 로드 완료");
    }

    static string Col(string[] cols, int idx) => idx >= 0 && idx < cols.Length ? cols[idx].Trim() : "";

    public void Edit()
    {

    }
#endif

}
[System.Serializable]
public class PhaseData
{
    public int phase;
    public bool isBoss;
    public int ordealLevel;
    public float enemyHp;
    public float enemyAttackPower;
    // public float time;
    public EnemyPatternData enemyPatternData;
}

[System.Serializable]
public class EnemyPatternData
{
    public EnemySpawnPatternData[] enemySpawnPatternDatas;
}

[System.Serializable]
public class EnemySpawnPatternData
{
    public int phase;
    public EnemyType enemyType;
    public Vector2Int countRange;
    public Vector2 intervalRange;

}