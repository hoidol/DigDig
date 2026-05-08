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
    public static readonly float[] WAVE_TIMES = { 60f, 60f, 60f, 60f };
    public static readonly float[] WAIT_WAVE_TIMES = { 40f, 90f, 90f, 90f };
    public static readonly int MAX_COUNT = 50;
    public string key;
    public string Title => key;
    public int level;
    public float difficulty;
    public float enemyHp; //가장 기본적의 기준 체력값
    public float enemyAttackPower; //가장 기본적의 기준 공격력
    public float[] initOreHps;
    public float[] increaseOreHpsPerIdx;

    public Boss boss;
    public UndergroundData[] undergroundDatas;
#if UNITY_EDITOR
    public void LoadData()
    {
        LoadStageBasic();
        LoadUndergroundsAndWaves();
        LoadEvent();
        EditorUtility.SetDirty(this);
        Debug.Log($"[StageData] {key} LoadData 완료 ({undergroundDatas.Length}개 층)");
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
        int iDifficulty = System.Array.IndexOf(headers, "difficulty");
        int iBossName = System.Array.IndexOf(headers, "bossName");
        int iEnemyHp = System.Array.IndexOf(headers, "enemyHp");
        int iEnemyAtk = System.Array.IndexOf(headers, "enemyAttackPower");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');
            if (Col(cols, iKey) != key) continue;

            if (int.TryParse(Col(cols, iLevel), out var lv)) level = lv;
            if (float.TryParse(Col(cols, iDifficulty), NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) difficulty = d;
            if (float.TryParse(Col(cols, iEnemyHp), NumberStyles.Float, CultureInfo.InvariantCulture, out var hp)) enemyHp = hp;
            if (float.TryParse(Col(cols, iEnemyAtk), NumberStyles.Float, CultureInfo.InvariantCulture, out var atk)) enemyAttackPower = atk;

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
            return;
        }
    }

    void LoadUndergroundsAndWaves()
    {
        string ugPath = Path.Combine(Application.dataPath, "Json/UndergroundData.csv");
        string wvPath = Path.Combine(Application.dataPath, "Json/WaveData.csv");
        if (!File.Exists(ugPath) || !File.Exists(wvPath)) return;

        string[] ugLines = File.ReadAllLines(ugPath, System.Text.Encoding.UTF8);
        string[] wvLines = File.ReadAllLines(wvPath, System.Text.Encoding.UTF8);
        if (ugLines.Length < 2 || wvLines.Length < 2) return;

        string[] ugH = ugLines[0].Split('\t');
        int uKey = System.Array.IndexOf(ugH, "stageKey");
        int uIdx = System.Array.IndexOf(ugH, "idx");
        int uBoss = System.Array.IndexOf(ugH, "isBoss");
        int uHpMul = System.Array.IndexOf(ugH, "hpMultiplierPerUnderground");
        int uAtkMul = System.Array.IndexOf(ugH, "AttackPowerMultiplierPerUnderground");
        int uInitOre = System.Array.IndexOf(ugH, "initOreHps");
        int uIncOre = System.Array.IndexOf(ugH, "increaseOreHpsPerWave");

        string[] wvH = wvLines[0].Split('\t');
        int wKey = System.Array.IndexOf(wvH, "stageKey");
        int wUgIdx = System.Array.IndexOf(wvH, "undergroundIdx");
        int wIdx = System.Array.IndexOf(wvH, "idx");
        int wPattern = System.Array.IndexOf(wvH, "patternType");
        int wHpAdd = System.Array.IndexOf(wvH, "hpAdd");
        int wAtkAdd = System.Array.IndexOf(wvH, "attackPowerAdd");

        var ugRows = new List<string[]>();
        for (int i = 1; i < ugLines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(ugLines[i])) continue;
            var c = ugLines[i].Split('\t');
            if (Col(c, uKey) == key) ugRows.Add(c);
        }
        ugRows.Sort((a, b) => int.Parse(Col(a, uIdx)).CompareTo(int.Parse(Col(b, uIdx))));

        var newUndergrounds = new UndergroundData[ugRows.Count];
        var newInitOreHps = new float[ugRows.Count];
        var newIncOreHps = new float[ugRows.Count];

        for (int u = 0; u < ugRows.Count; u++)
        {
            var c = ugRows[u];
            int.TryParse(Col(c, uIdx), out var idx);

            UndergroundData existing = (undergroundDatas != null && idx < undergroundDatas.Length) ? undergroundDatas[idx] : null;
            var ug = existing ?? new UndergroundData();
            ug.idx = idx;
            ug.isBoss = Col(c, uBoss).Equals("TRUE", System.StringComparison.OrdinalIgnoreCase);
            if (float.TryParse(Col(c, uHpMul), NumberStyles.Float, CultureInfo.InvariantCulture, out var hm)) ug.hpMultiplier = hm;
            if (float.TryParse(Col(c, uAtkMul), NumberStyles.Float, CultureInfo.InvariantCulture, out var am)) ug.attackPowerMultiplier = am;

            if (float.TryParse(Col(c, uInitOre), NumberStyles.Float, CultureInfo.InvariantCulture, out var io)) newInitOreHps[u] = io;
            if (float.TryParse(Col(c, uIncOre), NumberStyles.Float, CultureInfo.InvariantCulture, out var ic)) newIncOreHps[u] = ic;

            var waveRows = new List<string[]>();
            for (int i = 1; i < wvLines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(wvLines[i])) continue;
                var wc = wvLines[i].Split('\t');
                if (Col(wc, wKey) == key && Col(wc, wUgIdx) == idx.ToString()) waveRows.Add(wc);
            }
            waveRows.Sort((a, b) => int.Parse(Col(a, wIdx)).CompareTo(int.Parse(Col(b, wIdx))));

            ug.waveDatas = new WaveData[waveRows.Count];
            for (int w = 0; w < waveRows.Count; w++)
            {
                var wc = waveRows[w];
                var wd = new WaveData();
                if (int.TryParse(Col(wc, wIdx), out var wi)) wd.idx = wi;
                if (System.Enum.TryParse<EnemyPatternType>(Col(wc, wPattern), out var pt)) wd.patternType = pt;
                if (float.TryParse(Col(wc, wHpAdd), NumberStyles.Float, CultureInfo.InvariantCulture, out var ha)) wd.hpAdd = ha;
                if (float.TryParse(Col(wc, wAtkAdd), NumberStyles.Float, CultureInfo.InvariantCulture, out var aa)) wd.attackPowerAdd = aa;
                ug.waveDatas[w] = wd;
            }

            newUndergrounds[u] = ug;
        }

        undergroundDatas = newUndergrounds;
        initOreHps = newInitOreHps;
        increaseOreHpsPerIdx = newIncOreHps;
    }

    void LoadEvent()
    {
        string path = Path.Combine(Application.dataPath, "Json/EventData.csv");
        if (!File.Exists(path)) { Debug.LogWarning($"[StageData] EventData.csv 없음: {path}"); return; }

        string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
        if (lines.Length < 2) return;

        string[] headers = lines[0].Split('\t');
        int iType = System.Array.IndexOf(headers, "eventType");
        int iUgIdx = System.Array.IndexOf(headers, "undergroundIdx");
        int iMode = System.Array.IndexOf(headers, "spawnMode");
        int iTrigger = System.Array.IndexOf(headers, "trigger");
        int iTriggerData = System.Array.IndexOf(headers, "triggerData");
        int iCooldown = System.Array.IndexOf(headers, "cooldown");
        int iMaxSpawn = System.Array.IndexOf(headers, "maxSpawnCount");
        int iMinDist = System.Array.IndexOf(headers, "minDistanceFromOrigin");

        var grouped = new Dictionary<int, List<EventData>>();
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');

            if (!int.TryParse(Col(cols, iUgIdx), out var ugIdx)) continue;
            if (!grouped.ContainsKey(ugIdx)) grouped[ugIdx] = new List<EventData>();

            var ev = new EventData();
            if (System.Enum.TryParse<EventType>(Col(cols, iType), out var et)) ev.eventType = et;
            if (System.Enum.TryParse<EventSpawnMode>(Col(cols, iMode), out var sm)) ev.spawnMode = sm;
            if (System.Enum.TryParse<EventSpawnTrigger>(Col(cols, iTrigger), out var trig)) ev.trigger = trig;
            if (float.TryParse(Col(cols, iCooldown), NumberStyles.Float, CultureInfo.InvariantCulture, out var cd)) ev.cooldown = cd;
            if (int.TryParse(Col(cols, iMaxSpawn), out var ms)) ev.maxSpawnCount = ms;
            if (float.TryParse(Col(cols, iMinDist), NumberStyles.Float, CultureInfo.InvariantCulture, out var md)) ev.minDistanceFromOrigin = md;

            string rawTriggerData = Col(cols, iTriggerData);
            bool isRandom = ev.eventType == EventType.RandomBox || ev.eventType == EventType.RandomMerchant;
            string[] commaParts = rawTriggerData.Split(',');
            string triggerData = commaParts[0].Trim();
            string probData = commaParts.Length > 1 ? commaParts[1].Trim()
                            : (isRandom ? triggerData : "");
            if (isRandom && commaParts.Length == 1) triggerData = "";

            switch (ev.trigger)
            {
                case EventSpawnTrigger.LowHp:
                case EventSpawnTrigger.DistanceReached:
                    if (float.TryParse(triggerData, NumberStyles.Float, CultureInfo.InvariantCulture, out var fv))
                    {
                        if (ev.trigger == EventSpawnTrigger.LowHp) ev.hpThreshold = fv;
                        else ev.triggerDistance = fv;
                    }
                    break;
                case EventSpawnTrigger.WaveEnd:
                    if (int.TryParse(triggerData, out var wv)) ev.triggerWave = wv;
                    break;
                case EventSpawnTrigger.Time:
                    string[] parts = triggerData.Split('/');
                    if (parts.Length == 2 &&
                        float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var tx) &&
                        float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var ty))
                        ev.time = new Vector2(tx, ty);
                    break;
            }

            if (isRandom && !string.IsNullOrEmpty(probData))
            {
                string[] pv = probData.Split('/');
                if (pv.Length == 3 &&
                    float.TryParse(pv[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var p0) &&
                    float.TryParse(pv[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var p1) &&
                    float.TryParse(pv[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var p2))
                    ev.probability = new Vector3(p0, p1, p2);
            }

            grouped[ugIdx].Add(ev);
        }

        if (undergroundDatas == null) return;
        foreach (var kv in grouped)
        {
            if (kv.Key < undergroundDatas.Length)
                undergroundDatas[kv.Key].eventDatas = kv.Value.ToArray();
        }
    }
    static string Col(string[] cols, int idx) => idx >= 0 && idx < cols.Length ? cols[idx].Trim() : "";

    public void Edit()
    {
        for (int i = 0; i < undergroundDatas.Length; i++)
        {
            undergroundDatas[i].idx = i;
            if (i == undergroundDatas.Length - 1)
            {
                undergroundDatas[i].isBoss = true;
            }
            else
            {
                undergroundDatas[i].isBoss = false;

            }
            for (int j = 0; j < undergroundDatas[i].waveDatas.Length; j++)
            {
                undergroundDatas[i].waveDatas[j].idx = j;
            }
        }
    }
#endif

}

[System.Serializable]
public class UndergroundData
{
    public int idx;
    // public float waveWaitTime;
    public float hpMultiplier = 1f;           // 층별 체력 배율
    public float attackPowerMultiplier = 1f;  // 층별 공격력 배율
    public bool isBoss;
    public WaveData[] waveDatas;
    public EventData[] eventDatas;
}

[System.Serializable]
public class EventData
{
    public EventType eventType;
    public EventSpawnMode spawnMode;
    public EventSpawnTrigger trigger;

    [Header("공통 제한")]
    public int maxSpawnCount = -1;            // -1 = 무제한
    public float cooldown = 120f;             // 재등장 최소 대기(초)
    public float minDistanceFromOrigin = 0f;  // 플레이어가 이 거리 이상일 때만 등장

    [Header("LowHp 전용")]
    [Range(0f, 1f)]
    public float hpThreshold = 0.3f;          // HP 비율이 이 이하일 때 트리거

    [Header("DistanceReached 전용")]
    public float triggerDistance = 50f;       // 처음 이 거리에 도달했을 때 등장

    [Header("WaveEnd 전용")]
    public int triggerWave;

    [Header("Time 전용 - 몇 초 후에 등장 시킬지")]
    public Vector2 time;

    [Header("RandomBox/RandomMerchant 전용 - Normal/Rare/Unique 확률")]
    public Vector3 probability;

    public Grade GetRandomGrade()
    {
        float total = probability.x + probability.y + probability.z;
        if (total <= 0f) return Grade.Normal;

        float roll = Random.Range(0f, total);
        if (roll < probability.x) return Grade.Normal;
        if (roll < probability.x + probability.y) return Grade.Rare;
        return Grade.Unique;
    }

    public EventType ResolveEventType()
    {
        if (eventType != EventType.RandomBox && eventType != EventType.RandomMerchant)
            return eventType;

        Grade grade = GetRandomGrade();
        if (eventType == EventType.RandomBox)
            return grade == Grade.Rare ? EventType.RareBox
                 : grade == Grade.Unique ? EventType.UniqueBox
                 : EventType.NormalBox;
        else
            return grade == Grade.Rare ? EventType.RareMerchant
                 : grade == Grade.Unique ? EventType.UniqueMerchant
                 : EventType.NormalMerchant;
    }
}

public enum EventSpawnTrigger
{
    WaveEnd,          // 웨이브 종료 시
    LowHp,            // 체력이 hpThreshold 이하로 떨어졌을 때
    DistanceReached,  // 플레이어가 triggerDistance에 처음 도달했을 때
    Time, //흐름 시간
}

[System.Serializable]
public class WaveData
{
    public int idx;
    public EnemyPatternType patternType;
    public float hpAdd;
    public float attackPowerAdd;

}

