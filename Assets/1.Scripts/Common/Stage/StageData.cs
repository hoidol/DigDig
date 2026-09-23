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

    public string key;
    public int order;
    public string Title => key;
    public int stage; //Mode
    public DifficultyType difficulty;
    public StageRewardData[] rewardDatas;

    public float stoneHp;
    public Enemy[] enemyPrefabs; //해당 스테이지의 등장할 원거리, 근거리 엘리트 등 적 설정
    Dictionary<EnemyType, Enemy> enemyPrefabDic = new Dictionary<EnemyType, Enemy>();
    public PhaseData phaseData;
    public EventData[] eventDatas;
    public Boss boss;

    public bool ChekcUnlock()
    {
        if(difficulty == DifficultyType.Normal && order == 0)
        {
            return true;
        }

        UserStage preStageUserStage = UserDataManager.Instance.userStageManager.GetUserStage(difficulty, order -1);
        //이전 단계했는지 확인
        if(order > 0)
        {
            if(preStageUserStage.clearCount <=0)
                return false;
        }
        
        int difficultyNum = (int)difficulty;

        //낮은 난이도 깼는지 확인
        if(difficultyNum > 0)
        {
            UserStage lowDifficultyUserStage = UserDataManager.Instance.userStageManager.GetUserStage((DifficultyType)difficultyNum-1, order);
            if(lowDifficultyUserStage.clearCount <=0)
                return false;
        }

        return true;
        
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
        for (int i = 0; i < headers.Length; i++) headers[i] = headers[i].Trim();
        int iOrder = System.Array.IndexOf(headers, "order");
        int iDifficulty = System.Array.IndexOf(headers, "difficulty");
        int iStage = System.Array.IndexOf(headers, "stage");
        int iStoneHp = System.Array.IndexOf(headers, "stoneHp");

        // 에셋 경로(Assets/.../StageData/{Difficulty}/{order}.asset)로 CSV 행을 식별
        // (key 필드는 CSV의 복합 key 컬럼과 형식이 달라 게임 전역 조회용 값을 유지해야 하므로 매칭에 쓰지 않음)
        string assetPath = AssetDatabase.GetAssetPath(this);
        string fileName = Path.GetFileNameWithoutExtension(assetPath);
        string folderName = Path.GetFileName(Path.GetDirectoryName(assetPath));

        if (!int.TryParse(fileName, out int expectedOrder) || !System.Enum.TryParse(folderName, out DifficultyType expectedDifficulty))
        {
            Debug.LogWarning($"[StageData] 에셋 경로에서 order/difficulty 파싱 실패: {assetPath}");
            return;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');

            if (!int.TryParse(Col(cols, iOrder), out var od) || od != expectedOrder) continue;
            if (!System.Enum.TryParse(Col(cols, iDifficulty), out DifficultyType dt) || dt != expectedDifficulty) continue;

            order = od;
            difficulty = dt;
            if (int.TryParse(Col(cols, iStage), out var st)) stage = st;
            if (float.TryParse(Col(cols, iStoneHp), NumberStyles.Float, CultureInfo.InvariantCulture, out float shp)) stoneHp = shp;
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

        int iKey = System.Array.IndexOf(headers, "key");
        int iEnemyHp = System.Array.IndexOf(headers, "enemyHp");
        int iEnemyIncreaseHp = System.Array.IndexOf(headers, "enemyIncreaseHp");
        int iEnemyAtk = System.Array.IndexOf(headers, "enemyAttackPower");
        int iEnemyIncreaseAtk = System.Array.IndexOf(headers, "enemyIncreaseAttackPower");

        // order당 PhaseData는 1개만 사용 (hp/atk는 enemyIncrease* * GameManager.phase 로 실시간 스케일링됨)
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');

            // key 형식: {stage}_{order}_{phase} (ex. Greed_0_0)
            string[] keyParts = Col(cols, iKey).Split('_');
            if (keyParts.Length != 3) continue;
            if (!int.TryParse(keyParts[1], out int rowOrder) || rowOrder != order) continue;

            var d = new PhaseData();
            if (float.TryParse(Col(cols, iEnemyHp), NumberStyles.Float, CultureInfo.InvariantCulture, out float eh)) d.enemyHp = eh;
            if (float.TryParse(Col(cols, iEnemyIncreaseHp), NumberStyles.Float, CultureInfo.InvariantCulture, out float eih)) d.enemyIncreaseHp = eih;
            if (float.TryParse(Col(cols, iEnemyAtk), NumberStyles.Float, CultureInfo.InvariantCulture, out float ea)) d.enemyAttackPower = ea;
            if (float.TryParse(Col(cols, iEnemyIncreaseAtk), NumberStyles.Float, CultureInfo.InvariantCulture, out float eia)) d.enemyIncreaseAttackPower = eia;

            d.enemyPatternData = new EnemyPatternData[GameSetting.BOSS_PHASE];
            for (int p = 0; p < GameSetting.BOSS_PHASE; p++)
            {
                d.enemyPatternData[p] = new EnemyPatternData { enemySpawnPatternDatas = FindEnemyPatternData(p) };
            }

            phaseData = d;
            break;
        }
        Debug.Log($"[StageData] {key} PhaseData 로드 완료");
    }

    EnemySpawnPatternData[] FindEnemyPatternData(int phase)
    {
        // Debug.Log($"FindEnemyPatternData {phase}");
        string path = Path.Combine(Application.dataPath, $"Json/EnemyPatternData/{difficulty}.csv");
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

        int iEnemy = System.Array.IndexOf(headers, "enemyType");
        // int iDaySpawnCount = System.Array.IndexOf(headers, "DaySpawnCount");
        // int iDayItvl = System.Array.IndexOf(headers, "DayIntervalTime");
        int iNightSpawnCount = System.Array.IndexOf(headers, "WaveSpawnCount");
        int iNightItvl = System.Array.IndexOf(headers, "WaveIntervalTime");

        // var dayList = new List<EnemySpawnPatternData>();
        var waveList = new List<EnemySpawnPatternData>();
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');
            //if (Col(cols, iStage) != key) continue;
            if (!int.TryParse(Col(cols, iPhase), out int ph) || ph != phase) continue;

            System.Enum.TryParse(Col(cols, iEnemy), out EnemyType et);

            // if (int.TryParse(Col(cols, iDaySpawnCount), out int dayCount) &&
            //     TryParseIntervalRange(Col(cols, iDayItvl), out Vector2 dayItvl))
            // {
            //     dayList.Add(new EnemySpawnPatternData { enemyType = et, spawnCount = dayCount, intervalRange = dayItvl });
            // }

            if (int.TryParse(Col(cols, iNightSpawnCount), out int nightCount) &&
                TryParseIntervalRange(Col(cols, iNightItvl), out Vector2 nightItvl))
            {
                waveList.Add(new EnemySpawnPatternData { enemyType = et, spawnCount = nightCount, intervalRange = nightItvl });
            }
        }

        if ( waveList.Count == 0) { Debug.LogWarning($"[StageData] EnemyPatternData stage={key} phase={phase} 데이터 없음"); return null; }

        return waveList.ToArray();
    }

    static bool TryParseIntervalRange(string raw, out Vector2 range)
    {
        range = default;
        string[] parts = raw.Split('/');
        if (parts.Length != 2) return false;
        if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float min)) return false;
        if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float max)) return false;
        range = new Vector2(min, max);
        return true;
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
    public float enemyHp;
    public float enemyIncreaseHp;
    public float enemyAttackPower;
    public float enemyIncreaseAttackPower;
    public EnemyPatternData[] enemyPatternData; //총 9개가 되야함
}

[System.Serializable]
public class EnemyPatternData
{
    public EnemySpawnPatternData[] enemySpawnPatternDatas;
}

[System.Serializable]
public class EnemySpawnPatternData
{
    public EnemyType enemyType;
    public int spawnCount;
    public Vector2 intervalRange;    

}

[System.Serializable]
public class StageRewardData: RewardData
{

    public string id; //0-4_reward,0-7_reward,0-10_reward
    public int phase; // 4 ,7, 10(클리어)
}

public enum DifficultyType :int
{
    Normal,
    Hard,
    Hell
}