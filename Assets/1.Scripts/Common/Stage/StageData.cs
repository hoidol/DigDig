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
    //Key	Order	Difficulty	Stage	BossName	EnemyHp	AddHpRatioPerWave	MultiHpPerWave	EnemyTypes	EnemySpawnOrder
    public string key;
    // public int order;
    public string Title => key;
    // public int stage; //Mode
    public DifficultyType difficulty;
    public StageRewardData[] rewardDatas;

    public float enemyHp;
    public float addHpRatioPerWave;
    public float multiHpPerWave = 1f;
    public Enemy[] enemyPrefabs; //해당 스테이지의 적들    
    Dictionary<KeyValuePair<EnemyType, DefenceType>, Enemy> enemyPrefabDic = new Dictionary<KeyValuePair<EnemyType, DefenceType>, Enemy>();
    public WaveData[] waveDatas;
    // public EventData[] eventDatas;
    public Boss boss;

    // 난이도마다 스테이지 하나. Normal은 항상 열림, 그 외는 한 단계 낮은 난이도를 클리어해야 열림
    public bool ChekcUnlock()
    {
        if (difficulty == DifficultyType.Normal)
            return true;

        UserStage lowDifficultyUserStage = UserDataManager.Instance.userStageManager.GetUserStage(difficulty - 1);
        return lowDifficultyUserStage != null && lowDifficultyUserStage.maxWave >= 49;
    }
    public void Init()
    {
        enemyPrefabDic.Clear();
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            Enemy enemy = enemyPrefabs[i];

            DefenceType defenceType = enemy.defenceType;
            enemy.enemyData = EnemyManager.GetEnemyData(enemy.enemyType, defenceType);
            var dicKey = new KeyValuePair<EnemyType, DefenceType>(enemy.enemyType, defenceType);
            if (!enemyPrefabDic.TryAdd(dicKey, enemy))
                Debug.LogWarning($"[StageData] {key} 중복 적 프리팹 무시: {enemy.name} ({enemy.enemyType}, {defenceType})");
            else
                Debug.Log($"[StageData] {key} 적 프리펩 딕셔너리 등록 : {enemy.name} ({enemy.enemyType}, {defenceType})");
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

    // wave에 적용할 구간: startWave <= wave 인 것 중 가장 늦게 시작하는 구간 (waveDatas는 startWave 오름차순)
    // ex) startWave 0, 9, 19 → wave 18은 9 구간
    public WaveData GetWaveData(int wave)
    {
        if (waveDatas == null || waveDatas.Length == 0)
            return null;

        WaveData result = waveDatas[0];
        foreach (WaveData waveData in waveDatas)
        {
            if (waveData.startWave > wave)
                break;
            result = waveData;
        }
        return result;
    }

    // wave의 방어 타입. Random이면 구간의 defenceTypes 중 하나 (호출할 때마다 새로 뽑으므로 wave 시작 시 한 번만 호출)
    public DefenceType GetWaveDefenceType(int wave)
    {
        WaveData waveData = GetWaveData(wave);
        return waveData != null ? waveData.GetDefenceType(wave) : DefenceType.Normal;
    }

    public static StageData GetStageData(DifficultyType difficultyType)
    {
        if (StageManager.Instance != null)
        {
            return StageManager.Instance.GetStageData(difficultyType);
        }
        return Resources.Load<StageData>($"StageData/{difficultyType}");
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

        // 난이도마다 스테이지 하나: 에셋 파일명(Assets/.../StageData/{Difficulty}.asset)으로 CSV 행을 식별, key = 난이도 이름
        string assetPath = AssetDatabase.GetAssetPath(this);
        string fileName = Path.GetFileNameWithoutExtension(assetPath);

        if (!System.Enum.TryParse(fileName, out DifficultyType expectedDifficulty))
        {
            Debug.LogWarning($"[StageData] 에셋 파일명에서 difficulty 파싱 실패: {assetPath}");
            return;
        }

        string bossName = "";
        foreach (TableReader.Row row in table.rows)
        {
            if (!row.TryGetEnum("Difficulty", out DifficultyType dt) || dt != expectedDifficulty) continue;

            difficulty = dt;
            key = dt.ToString();
            if (row.TryGetFloat("EnemyHp", out float eh)) enemyHp = eh;
            if (row.TryGetFloat("AddHpRatioPerWave", out float ahr)) addHpRatioPerWave = ahr;
            if (row.TryGetFloat("MultiHpPerWave", out float mh)) multiHpPerWave = mh;
            bossName = row.Get("BossName");
            break;
        }

        LoadWaveDatas();
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

    const string ENEMY_PREFAB_FOLDER = "Assets/3.Prefabs/Stage/0/Enemy"; // 모든 난이도 공용 (난이도 차이는 체력 배율로)

    void LoadEnemyPrefabs()
    {
        string folderPath = ENEMY_PREFAB_FOLDER;
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

    // WaveData/{difficulty}.csv 한 행 = startWave부터 시작하는 구간 1개 (StartWave, DefenceTypes, DefenceWaveTypes)
    void LoadWaveDatas()
    {
        TableReader table = TableReader.Load($"WaveData/{difficulty}.csv");
        if (table == null || table.rows.Count == 0) return;

        var list = new List<WaveData>();
        foreach (TableReader.Row row in table.rows)
        {
            if (!row.TryGetInt("StartWave", out int startWave)) continue;

            var d = new WaveData();
            d.startWave = startWave;
            d.defenceTypes = ParseEnumList<DefenceType>(row.Get("DefenceTypes"), "DefenceTypes");
            d.defenceWaveTypes = ParseEnumList<DefenceWaveType>(row.Get("DefenceWaveTypes"), "DefenceWaveTypes");
            list.Add(d);
        }

        list.Sort((a, b) => a.startWave.CompareTo(b.startWave)); // GetWaveData가 오름차순 전제
        waveDatas = list.ToArray();
        Debug.Log($"[StageData] {key} WaveData {waveDatas.Length}개 로드 완료");
    }
    public void Edit()
    {

    }
#endif

}


[System.Serializable]
public class StageRewardData : RewardData
{

    public string id; //0-4_reward,0-7_reward,0-10_reward
    public int wave; // 4 ,7, 10(클리어)
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
    // 4: 중갑형(HeavyArmor) 삭제. 기존 직렬화 값 유지를 위해 번호 비워둠
    Organic = 5, //생체형 소환
}