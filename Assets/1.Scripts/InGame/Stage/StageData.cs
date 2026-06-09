using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
#endif


public class StageData : MonoBehaviour
{
    public SpecialEnemySpawner[] specialEnemySpawners;
    public static readonly int MAX_ENEMY_COUNT = 50;
    public string key;
    public string Title => key;
    public int level;

    public float oreHp;
    public PhaseData[] phaseDatas;
    public EventData[] eventDatas;
    public Boss boss;
    public PhaseData GetPhaseData(int idx = -1)
    {
        if (idx < 0)
            idx = GameManager.Instance.phase;
        return phaseDatas[idx];
    }

    void Start()
    {
        specialEnemySpawners = GetComponentsInChildren<SpecialEnemySpawner>();
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
        int iBossName = System.Array.IndexOf(headers, "bossName");

        int iOreHp = System.Array.IndexOf(headers, "oreHp");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');
            if (Col(cols, iKey) != key) continue;

            if (int.TryParse(Col(cols, iLevel), out var lv)) level = lv;
            if (float.TryParse(Col(cols, iOreHp), NumberStyles.Float, CultureInfo.InvariantCulture, out float oh)) oreHp = oh;

            string bossName = Col(cols, iBossName);
            if (!string.IsNullOrEmpty(bossName))
            {
                foreach (string guid in AssetDatabase.FindAssets($"{bossName} t:Prefab"))
                {
                    string p = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetFileNameWithoutExtension(p) != bossName) continue;
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                    var b = go != null ? go.GetComponent<Boss>() : null;
                    if (b != null) { boss = b; break; }
                }
            }
            break;
        }

        LoadPhaseDatas();
        LoadEventDatas();
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
        int iTime = System.Array.IndexOf(headers, "time");

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
            if (float.TryParse(Col(cols, iTime), NumberStyles.Float, CultureInfo.InvariantCulture, out float pt)) d.time = pt;

            d.enemyPatternData = FindEnemyPatternData(d.phase);

            list.Add(d);
        }

        phaseDatas = list.ToArray();
        Debug.Log($"[StageData] {key} PhaseData {list.Count}개 로드 완료");
    }

    EnemyPatternData FindEnemyPatternData(int phase)
    {
        Debug.Log($"FindEnemyPatternData {phase}");
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
            if (float.TryParse(Col(cols, iTrigger), NumberStyles.Float, CultureInfo.InvariantCulture, out float tr)) e.triggerTime = tr;
            if (float.TryParse(Col(cols, iEnd), NumberStyles.Float, CultureInfo.InvariantCulture, out float en)) e.endTime = en;
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
    public float time;
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
    public float triggerTime;
    public float endTime;
    public EnemyType enemyType;
    public Vector2Int countRange;
    public Vector2 intervalRange;

}