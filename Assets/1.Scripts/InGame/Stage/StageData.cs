using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "StageData", menuName = "StageData", order = 0)]
public class StageData : ScriptableObject
{
    // public static readonly float[] WAVE_TIMES = { 60f, 60f, 60f, 60f };
    // public static readonly float[] WAIT_WAVE_TIMES = { 40f, 90f, 90f, 90f };
    public static readonly int MAX_ENEMY_COUNT = 50;
    public string key;
    public string Title => key;
    public int level;

    public float oreHp;
    public OrdealProgressData[] ordealProgressDatas;
    public EventData[] eventDatas;

    public Boss boss;
    public OrdealProgressData GetOrdealProgressData(int count = -1)
    {
        if (count < 0)
            count = GameManager.Instance.ordealClearCount;
        return ordealProgressDatas[count];
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

        LoadOrdealProgressDatas();
        LoadEventDatas();
    }

    void LoadOrdealProgressDatas()
    {
        string path = Path.Combine(Application.dataPath, "Json/OrdealProgressData.csv");
        if (!File.Exists(path)) { Debug.LogWarning($"[StageData] OrdealProgressData CSV 없음: {path}"); return; }

        string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
        if (lines.Length < 2) return;

        string[] headers = lines[0].Split('\t');
        for (int i = 0; i < headers.Length; i++) headers[i] = headers[i].Trim();

        int iClearCount = System.Array.IndexOf(headers, "clearCount");
        int iIsBoss = System.Array.IndexOf(headers, "isBoss");
        int iOrdealLevels = System.Array.IndexOf(headers, "ordealLevels");
        int iEnemyHp = System.Array.IndexOf(headers, "enemyHp");
        int iEnemyAtk = System.Array.IndexOf(headers, "enemyAttackPower");

        var list = new List<OrdealProgressData>();
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');

            var d = new OrdealProgressData();
            if (int.TryParse(Col(cols, iClearCount), out int cc)) d.clearCount = cc;
            d.isBoss = Col(cols, iIsBoss).ToUpper() == "TRUE";


            if (int.TryParse(Col(cols, iOrdealLevels), out int ol)) d.ordealLevel = ol;


            if (float.TryParse(Col(cols, iEnemyHp), NumberStyles.Float, CultureInfo.InvariantCulture, out float eh)) d.enemyHp = eh;
            if (float.TryParse(Col(cols, iEnemyAtk), NumberStyles.Float, CultureInfo.InvariantCulture, out float ea)) d.enemyAttackPower = ea;
            list.Add(d);
        }

        ordealProgressDatas = list.ToArray();
        Debug.Log($"[StageData] {key} OrdealProgressData {list.Count}개 로드 완료");
    }

    void LoadEventDatas()
    {
        string path = Path.Combine(Application.dataPath, "Json/EventData.csv");
        if (!File.Exists(path)) { Debug.LogWarning($"[StageData] EventData CSV 없음: {path}"); return; }

        string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
        if (lines.Length < 2) return;

        string[] headers = lines[0].Split(',');
        for (int i = 0; i < headers.Length; i++) headers[i] = headers[i].Trim();

        int iTypes = System.Array.IndexOf(headers, "eventTypes");
        int iChances = System.Array.IndexOf(headers, "chances");
        int iTriggers = System.Array.IndexOf(headers, "triggers");
        int iOrdeal = System.Array.IndexOf(headers, "ordealClearCount");
        int iHpThresh = System.Array.IndexOf(headers, "hpThreshold");
        int iDist = System.Array.IndexOf(headers, "distancesFromPlayer");

        var list = new List<EventData>();
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split(',');

            var d = new EventData();

            // eventTypes
            string typesRaw = Col(cols, iTypes);
            if (!string.IsNullOrEmpty(typesRaw))
            {
                string[] typeStrs = typesRaw.Split('/');
                var typeList = new List<EventType>();
                foreach (var t in typeStrs)
                    if (System.Enum.TryParse(t.Trim(), out EventType et)) typeList.Add(et);
                d.eventTypes = typeList.ToArray();
            }

            // chances
            string chancesRaw = Col(cols, iChances);
            if (!string.IsNullOrEmpty(chancesRaw))
            {
                string[] chanceStrs = chancesRaw.Split('/');
                var chanceList = new List<float>();
                foreach (var c in chanceStrs)
                    if (float.TryParse(c.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float cv)) chanceList.Add(cv);
                d.chances = chanceList.ToArray();
            }

            // triggers
            string triggersRaw = Col(cols, iTriggers);
            if (!string.IsNullOrEmpty(triggersRaw))
            {
                string[] trigStrs = triggersRaw.Split('/');
                var trigList = new List<EventTrigger>();
                foreach (var t in trigStrs)
                    if (System.Enum.TryParse(t.Trim(), out EventTrigger et)) trigList.Add(et);
                d.triggers = trigList.ToArray();
            }

            if (int.TryParse(Col(cols, iOrdeal), out int oc)) d.ordealClearCount = oc;
            if (float.TryParse(Col(cols, iHpThresh), NumberStyles.Float, CultureInfo.InvariantCulture, out float hp)) d.hpThreshold = hp;

            string distRaw = Col(cols, iDist);
            if (!string.IsNullOrEmpty(distRaw))
            {
                string[] distParts = distRaw.Split('/');
                float x = 0f, y = 0f;
                if (distParts.Length > 0) float.TryParse(distParts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out x);
                if (distParts.Length > 1) float.TryParse(distParts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out y);
                d.distanceFromPlayer = new Vector2(x, y);
            }

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
public class OrdealProgressData
{
    public int clearCount;
    public bool isBoss;
    public int ordealLevel;
    public float enemyHp;
    public float enemyAttackPower;
}