using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "StageData", menuName = "StageData")]
public class StageData : ScriptableObject
{
    public EnemySpawnerContainer enemySpawnerContainerPrefab;
    //Key	Order	Difficulty	Stage	BossName	EnemyHp	AddHpRatioPerPhase	MultiHpPerPhase	EnemyTypes	EnemySpawnOrder
    public string key;
    public int order;
    public string Title => key;
    public int stage; //Mode
    public DifficultyType difficulty;
    public StageRewardData[] rewardDatas;

    public float enemyHp;
    public float addHpRatioPerPhase;
    public float multiHpPerPhase = 1f;
    public Enemy[] enemyPrefabs; //해당 스테이지의 적들
    public DefenceType[] defenceTypes; // 해당 스테이지에서 등장하는 방어 타입
    public DefenceWaveType[] defenceWaveTypes;// phase마다 순서대로 적용 (phase % 개수), 한 phase 안에서는 모두 같은 방어 타입
    Dictionary<KeyValuePair<EnemyType, DefenceType>, Enemy> enemyPrefabDic = new Dictionary<KeyValuePair<EnemyType, DefenceType>, Enemy>();
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
        enemyPrefabDic.Clear();
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            Enemy enemy = enemyPrefabs[i];
            DefenceType defenceType = enemy.enemyData != null ? enemy.enemyData.defenceType : DefenceType.Normal;
            var dicKey = new KeyValuePair<EnemyType, DefenceType>(enemy.enemyType, defenceType);
            if (!enemyPrefabDic.TryAdd(dicKey, enemy))
                Debug.LogWarning($"[StageData] {key} 중복 적 프리팹 무시: {enemy.name} ({enemy.enemyType}, {defenceType})");
        }
    }

    // 해당 방어 타입 프리팹이 없으면 Normal 방어 타입으로 대체
    public Enemy GetEnemyPrefab(EnemyType enemyType, DefenceType defenceType = DefenceType.Normal)
    {
        if (enemyPrefabDic.TryGetValue(new KeyValuePair<EnemyType, DefenceType>(enemyType, defenceType), out Enemy enemy))
            return enemy;

        Debug.LogWarning($"[StageData] {key} 적 프리팹 없음 ({enemyType}, {defenceType}) → Normal로 대체");
        enemyPrefabDic.TryGetValue(new KeyValuePair<EnemyType, DefenceType>(enemyType, DefenceType.Normal), out enemy);
        return enemy;
    }

    // phase의 방어 타입. Random이면 defenceTypes 중 하나 (호출할 때마다 새로 뽑으므로 phase 시작 시 한 번만 호출)
    public DefenceType GetPhaseDefenceType(int phase)
    {
        if (defenceWaveTypes == null || defenceWaveTypes.Length == 0)
            return DefenceType.Normal;

        DefenceWaveType waveType = defenceWaveTypes[phase % defenceWaveTypes.Length];
        if (waveType == DefenceWaveType.Random)
        {
            if (defenceTypes == null || defenceTypes.Length == 0)
                return DefenceType.Normal;
            return defenceTypes[Random.Range(0, defenceTypes.Length)];
        }

        // DefenceWaveType은 Random 다음부터 DefenceType과 같은 이름 순서
        return (DefenceType)System.Enum.Parse(typeof(DefenceType), waveType.ToString());
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
            if (row.TryGetFloat("EnemyHp", out float eh)) enemyHp = eh;
            if (row.TryGetFloat("AddHpRatioPerPhase", out float ahr)) addHpRatioPerPhase = ahr;
            if (row.TryGetFloat("MultiHpPerPhase", out float mh)) multiHpPerPhase = mh;
            defenceTypes = ParseEnumList<DefenceType>(row.Get("DefenceTypes"), "DefenceTypes");
            defenceWaveTypes = ParseEnumList<DefenceWaveType>(row.Get("DefenceWaveTypes"), "DefenceWaveTypes");
            bossName = row.Get("BossName");
            break;
        }

        LoadPhaseDatas();
        // LoadEventDatas();
        LoadEnemyPrefabs();
        LoadBossPrefab(bossName);
        LoadEnemySpawnerContainerPrefab();
    }

    // "A/B/C" → enum 배열 (파싱 실패한 값은 경고 후 제외)
    T[] ParseEnumList<T>(string raw, string column) where T : struct, System.Enum
    {
        var list = new List<T>();
        if (string.IsNullOrEmpty(raw))
            return list.ToArray();

        foreach (var s in raw.Split('/'))
        {
            if (System.Enum.TryParse(s.Trim(), out T v)) list.Add(v);
            else Debug.LogWarning($"[StageData] {key} {column} 파싱 실패: {s}");
        }
        return list.ToArray();
    }

    const string ENEMY_SPAWNER_CONTAINER_PREFAB_PATH = "Assets/3.Prefabs/Stage/EnemySpawnerContainer.prefab";

    // enemySpawnerContainerPrefab이 비어 있으면 기본 프리팹으로 채움 (이미 지정돼 있으면 유지)
    void LoadEnemySpawnerContainerPrefab()
    {
        if (enemySpawnerContainerPrefab != null)
            return;

        var go = AssetDatabase.LoadAssetAtPath<GameObject>(ENEMY_SPAWNER_CONTAINER_PREFAB_PATH);
        var container = go != null ? go.GetComponent<EnemySpawnerContainer>() : null;
        if (container == null)
        {
            Debug.LogWarning($"[StageData] {key} EnemySpawnerContainer 프리팹 없음: {ENEMY_SPAWNER_CONTAINER_PREFAB_PATH}");
            return;
        }

        enemySpawnerContainerPrefab = container;
        Debug.Log($"[StageData] {key} EnemySpawnerContainer 로드 완료");
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

    // WaveData/{difficulty}.csv의 Phase 행마다 PhaseData 1개 (waveType)
    void LoadPhaseDatas()
    {
        TableReader table = TableReader.Load($"WaveData/{difficulty}.csv");
        if (table == null || table.rows.Count == 0) return;

        var list = new List<PhaseData>();
        foreach (TableReader.Row row in table.rows)
        {
            if (!row.TryGetInt("Phase", out int phase)) continue;

            var d = new PhaseData();
            if (row.TryGetEnum("WaveType", out WaveType wt)) d.waveType = wt;
            else Debug.LogWarning($"[StageData] WaveData WaveType 파싱 실패: {row.Get("WaveType")} (phase={phase}) → Normal");

            // phase 순서대로 배치 (phaseDatas[phase])
            while (list.Count <= phase) list.Add(null);
            list[phase] = d;
        }

        phaseDatas = list.ToArray();
        Debug.Log($"[StageData] {key} PhaseData {phaseDatas.Length}개 로드 완료");
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
    public WaveType waveType; // 이 phase에 실행되는 웨이브
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


public enum DefenceWaveType
{
    
    Random,
    Normal, //일반형 소환
    Armored, //갑옷형 소환
    Ethereal, //정령형 소환 
    HeavyArmor, // 중갑형 소환
    Organic, //생체형 소환
}