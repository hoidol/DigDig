using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
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
    public float enemyHp;
    public float addHpRatioPerPhase;
    public float enemyAtk;
    public float addAtkRatioPerPhase;
    public float multiHpPerPhase = 1f;
    public float multiAtkPerPhase = 1f;
    public Enemy[] enemyPrefabs; //해당 스테이지의 등장할 원거리, 근거리 엘리트 등 적 설정
    Dictionary<EnemyType, Enemy> enemyPrefabDic = new Dictionary<EnemyType, Enemy>();
    public PhaseData[] phaseDatas;
    // public EventData[] eventDatas;
    public Boss boss;

    public bool ChekcUnlock()
    {
        if (difficulty == DifficultyType.Normal && order == 0)
        {
            return true;
        }

        UserStage preStageUserStage = UserDataManager.Instance.userStageManager.GetUserStage(difficulty, order - 1);
        //이전 단계했는지 확인
        if (order > 0)
        {
            if (preStageUserStage.clearCount <= 0)
                return false;
        }

        int difficultyNum = (int)difficulty;

        //낮은 난이도 깼는지 확인
        if (difficultyNum > 0)
        {
            UserStage lowDifficultyUserStage = UserDataManager.Instance.userStageManager.GetUserStage((DifficultyType)difficultyNum - 1, order);
            if (lowDifficultyUserStage.clearCount <= 0)
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
        TableReader table = TableReader.Load("StageData.csv");
        if (table == null || table.rows.Count == 0) return;

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

        string bossName = "";
        foreach (TableReader.Row row in table.rows)
        {
            if (!row.TryGetInt("Order", out int od) || od != expectedOrder) continue;
            if (!row.TryGetEnum("Difficulty", out DifficultyType dt) || dt != expectedDifficulty) continue;

            order = od;
            difficulty = dt;
            if (row.TryGetInt("Stage", out int st)) stage = st;
            if (row.TryGetFloat("StoneHp", out float shp)) stoneHp = shp;
            if (row.TryGetFloat("EnemyHp", out float eh)) enemyHp = eh;
            if (row.TryGetFloat("AddHpRatioPerPhase", out float ahr)) addHpRatioPerPhase = ahr;
            if (row.TryGetFloat("EnemyAtk", out float ea)) enemyAtk = ea;
            if (row.TryGetFloat("AddAtkRatioPerPhase", out float aar)) addAtkRatioPerPhase = aar;
            if (row.TryGetFloat("MultiHpPerPhase", out float mh)) multiHpPerPhase = mh;
            if (row.TryGetFloat("MultiAtkPerPhase", out float ma)) multiAtkPerPhase = ma;
            bossName = row.Get("BossName");
            break;
        }

        LoadPhaseDatas();
        // LoadEventDatas();
        LoadEnemyPrefabs();
        LoadBossPrefab(bossName);
    }

    const string BOSS_PREFAB_FOLDER = "Assets/3.Prefabs/Boss";

    // StageData.csv의 BossName → Assets/3.Prefabs/Boss/{BossName}.prefab (못 찾으면 기존 값 유지)
    void LoadBossPrefab(string bossName)
    {
        if (string.IsNullOrEmpty(bossName))
        {
            Debug.LogWarning($"[StageData] {key} BossName 비어 있음 (기존 보스 유지: {(boss != null ? boss.name : "없음")})");
            return;
        }

        string prefabPath = $"{BOSS_PREFAB_FOLDER}/{bossName}.prefab";
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var b = go != null ? go.GetComponent<Boss>() : null;
        if (b == null)
        {
            Debug.LogWarning($"[StageData] {key} 보스 프리팹 없음: {prefabPath} (기존 보스 유지: {(boss != null ? boss.name : "없음")})");
            return;
        }

        boss = b;
        Debug.Log($"[StageData] {key} BossPrefab 로드 완료: {boss.name}");
    }

    void LoadEnemyPrefabs()
    {
        string folderPath = $"Assets/3.Prefabs/Stage/{order}/Enemy";
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

    // WaveData/{difficulty}.csv의 Phase 행마다 PhaseData 1개 (waveTypes + 해당 phase의 EnemyPatternData)
    void LoadPhaseDatas()
    {
        TableReader table = TableReader.Load($"WaveData/{difficulty}.csv");
        if (table == null || table.rows.Count == 0) return;

        var list = new List<PhaseData>();
        foreach (TableReader.Row row in table.rows)
        {
            if (!row.TryGetInt("Phase", out int phase)) continue;

            var d = new PhaseData { waveTypes = new List<WaveType>() };
            string typesRaw = row.Get("WaveType");
            if (!string.IsNullOrEmpty(typesRaw))
            {
                foreach (var t in typesRaw.Split('/'))
                {
                    if (System.Enum.TryParse(t.Trim(), out WaveType wt)) d.waveTypes.Add(wt);
                    else Debug.LogWarning($"[StageData] WaveData WaveType 파싱 실패: {t} (phase={phase})");
                }
            }
            if (row.TryGetFloat("PeakCountMul", out float peak)) d.peakCountMultiplier = peak;
            if (bool.TryParse(row.Get("SpecialPattern"), out bool special)) d.specialPattern = special;
            d.enemySpawnPatternDatas = FindEnemyPatternData(phase);

            // phase 순서대로 배치 (phaseDatas[phase])
            while (list.Count <= phase) list.Add(null);
            list[phase] = d;
        }

        phaseDatas = list.ToArray();
        Debug.Log($"[StageData] {key} PhaseData {phaseDatas.Length}개 로드 완료");
    }

    EnemySpawnPatternData[] FindEnemyPatternData(int phase)
    {
        TableReader table = TableReader.Load($"EnemyPatternData/{difficulty}.csv");
        if (table == null) return null;
        if (table.rows.Count == 0)
        {
            Debug.LogWarning($"[StageData] EnemyPatternData 행 없음: {table.path}");
            return null;
        }

        // 헤더: phase / enemyType / WaveSpawnCounts(min/max) / WaveIntervalTimes(min/max)
        if (!table.HasColumn("Phase") || !table.HasColumn("EnemyType") || !table.HasColumn("WaveSpawnCounts") || !table.HasColumn("WaveIntervalTimes"))
        {
            Debug.LogWarning($"[StageData] EnemyPatternData 헤더 누락: {table.path}");
            return null;
        }

        var waveList = new List<EnemySpawnPatternData>();
        foreach (TableReader.Row row in table.rows)
        {
            // 난이도별 파일이므로 phase 로 행 식별
            if (!row.TryGetInt("Phase", out int ph) || ph != phase) continue;

            if (!row.TryGetEnum("EnemyType", out EnemyType et))
            {
                Debug.LogWarning($"[StageData] EnemyPatternData enemyType 파싱 실패: {row.Get("EnemyType")} ({difficulty}.csv {row.lineNumber}행)");
                continue;
            }

            if (TryParseCountRange(row.Get("WaveSpawnCounts"), out Vector2Int waveCount) &&
                TryParseIntervalRange(row.Get("WaveIntervalTimes"), out Vector2 waveItvl))
            {
                waveList.Add(new EnemySpawnPatternData { enemyType = et, spawnCountRange = waveCount, intervalRange = waveItvl });
            }
            else
            {
                Debug.LogWarning($"[StageData] EnemyPatternData 파싱 실패: {row.Get("WaveSpawnCounts")} / {row.Get("WaveIntervalTimes")} ({difficulty}.csv {row.lineNumber}행)");
            }
        }

        if (waveList.Count == 0) { Debug.LogWarning($"[StageData] EnemyPatternData {difficulty} phase={phase} 데이터 없음"); return null; }

        return waveList.ToArray();
    }

    // "min/max" 또는 단일 값 "n" 허용
    static bool TryParseCountRange(string raw, out Vector2Int range)
    {
        range = default;
        string[] parts = raw.Split('/');
        if (parts.Length == 1 && int.TryParse(parts[0], out int single)) { range = new Vector2Int(single, single); return true; }
        if (parts.Length != 2) return false;
        if (!int.TryParse(parts[0], out int min)) return false;
        if (!int.TryParse(parts[1], out int max)) return false;
        range = new Vector2Int(min, max);
        return true;
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

    // void LoadEventDatas()
    // {
    //     TableReader table = TableReader.Load("EventData.csv");
    //     if (table == null || table.rows.Count == 0) return;

    //     var list = new List<EventData>();
    //     foreach (TableReader.Row row in table.rows)
    //     {
    //         var d = new EventData();

    //         string typesRaw = row.Get("EventTypes");
    //         if (!string.IsNullOrEmpty(typesRaw))
    //         {
    //             var typeList = new List<EventType>();
    //             foreach (var t in typesRaw.Split('/'))
    //                 if (System.Enum.TryParse(t.Trim(), out EventType et)) typeList.Add(et);
    //             d.eventTypes = typeList.ToArray();
    //         }

    //         string chancesRaw = row.Get("Chances");
    //         if (!string.IsNullOrEmpty(chancesRaw))
    //         {
    //             var chanceList = new List<float>();
    //             foreach (var c in chancesRaw.Split('/'))
    //                 if (float.TryParse(c.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float cv)) chanceList.Add(cv);
    //             d.chances = chanceList.ToArray();
    //         }

    //         string triggersRaw = row.Get("Triggers");
    //         if (!string.IsNullOrEmpty(triggersRaw))
    //         {
    //             var trigList = new List<EventTrigger>();
    //             foreach (var t in triggersRaw.Split('/'))
    //                 if (System.Enum.TryParse(t.Trim(), out EventTrigger et)) trigList.Add(et);
    //             d.triggers = trigList.ToArray();
    //         }

    //         if (row.TryGetInt("PhaseIdx", out int pi)) d.phaseIdx = pi;

    //         list.Add(d);
    //     }

    //     eventDatas = list.ToArray();
    //     Debug.Log($"[StageData] {key} EventData {list.Count}개 로드 완료");
    // }

    public void Edit()
    {

    }
#endif

}
[System.Serializable]
public class PhaseData
{
    public EnemySpawnPatternData[] enemySpawnPatternDatas;
    public List<WaveType> waveTypes; // 이 중 하나가 실행됨
    public float peakCountMultiplier = 1f; // 웨이브 끝에 도달하는 스폰 수 배율
    public bool specialPattern; // BattleWave 특수 스폰(specialEnemySpawners) 동작 여부
}

[System.Serializable]
public class EnemySpawnPatternData
{
    public EnemyType enemyType;
    public Vector2Int spawnCountRange; //min/max (둘 다 포함)
    public Vector2 intervalRange;

    public int GetSpawnCount() => Random.Range(spawnCountRange.x, spawnCountRange.y + 1);

}

[System.Serializable]
public class StageRewardData : RewardData
{

    public string id; //0-4_reward,0-7_reward,0-10_reward
    public int phase; // 4 ,7, 10(클리어)
}

public enum DifficultyType : int
{
    Normal,
    Hard,
    Hell
}
