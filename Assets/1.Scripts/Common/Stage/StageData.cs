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
    // 스테이지 order(0부터, 화면 표시 = order + 1) × 난이도 = 스테이지 하나. key = "{난이도}_{order}" (예: Normal_0 = 스테이지 1 보통)
    public int order;
    public string key => MakeKey(order, difficulty);
    public string Title => key;
    public int DisplayNumber => order + 1;
    public static string MakeKey(int order, DifficultyType difficulty) => $"{difficulty}_{order}";
    public const int FIRST_STAGE_ORDER = 0;
    // public int stage; //Mode
    public DifficultyType difficulty;
    public StageRewardData[] rewardDatas;

    public float enemyHp;
    public float addHpRatioPerWave;
    public float multiHpPerWave = 1f;
    // 20 wave(보스 주기) 구간별 체력 배율. [0] = wave 1~20, [1] = 21~40 … 칸이 모자라면 마지막 값.
    // StageData.csv CycleHpMuls("1/1.1/1.21/1.33"). 비우면 GameSetting.HP_MUL_PER_BOSS_CYCLE을 구간마다 곱함
    public float[] cycleHpMuls;

    public float GetCycleHpMul(int waveIdx)
    {
        int cycle = Mathf.Max(0, waveIdx) / GameSetting.BOSS_WAVE_INTERVAL;
        if (cycleHpMuls == null || cycleHpMuls.Length == 0)
            return Mathf.Pow(GameSetting.HP_MUL_PER_BOSS_CYCLE, cycle);
        return cycleHpMuls[Mathf.Min(cycle, cycleHpMuls.Length - 1)];
    }
    // 일반 적 방어 타입 순서. wave 1부터 한 칸씩, 끝나면 처음부터 반복 (StageData.csv DefenceWaveTypes "Normal/Normal/Ethereal/...")
    // wave 구간별로 바뀌는 것 없음. 비우면 DEFAULT_DEFENCE_WAVE_TYPES. Random = 그 wave 번호로 정해지는 랜덤(Light/Normal/Armored/Ethereal 중)
    public DefenceWaveType[] defenceWaveTypes;
    public static readonly DefenceWaveType[] DEFAULT_DEFENCE_WAVE_TYPES =
    {
        DefenceWaveType.Light, DefenceWaveType.Normal, DefenceWaveType.Ethereal,
        DefenceWaveType.Light, DefenceWaveType.Normal, DefenceWaveType.Armored,
    };

    public DefenceType GetWaveDefenceType(int waveIdx) => GetDefenceType(defenceWaveTypes, waveIdx);

    public static DefenceType GetDefenceType(DefenceWaveType[] cycle, int waveIdx)
    {
        if (cycle == null || cycle.Length == 0)
            cycle = DEFAULT_DEFENCE_WAVE_TYPES;
        waveIdx = Mathf.Max(0, waveIdx);
        DefenceWaveType waveType = cycle[waveIdx % cycle.Length];
        if (waveType == DefenceWaveType.Random)
        {
            // 미리보기(다음 wave 정보)와 실제 wave가 같도록 wave 번호로 고정된 랜덤
            DefenceType[] candidates = { DefenceType.Light, DefenceType.Normal, DefenceType.Armored, DefenceType.Ethereal };
            return candidates[new System.Random(waveIdx * 7919 + 17).Next(candidates.Length)];
        }
        // DefenceWaveType은 Random 다음부터 DefenceType과 같은 이름
        return (DefenceType)System.Enum.Parse(typeof(DefenceType), waveType.ToString());
    }

    public Enemy[] enemyPrefabs; //해당 스테이지의 적들    
    Dictionary<KeyValuePair<EnemyType, DefenceType>, Enemy> enemyPrefabDic = new Dictionary<KeyValuePair<EnemyType, DefenceType>, Enemy>();
    // public EventData[] eventDatas;
    public Boss boss;

    // 해금 조건 = 이전 단계 클리어(마지막 wave(표시 번호)까지 깸) (2026-11-19, 이전: wave 100 도달)
    public const int UNLOCK_WAVE = GameSetting.LAST_WAVE;

    // 스테이지 1 Normal은 항상 열림.
    // 같은 스테이지의 Hard/Hell = 그 스테이지의 한 단계 낮은 난이도 클리어, 스테이지 N(2~) Normal = 스테이지 N-1 Normal 클리어
    public bool ChekcUnlock()
    {
        if (order <= FIRST_STAGE_ORDER && difficulty == DifficultyType.Normal)
            return true;
        return IsUnlocked(GetUnlockPrevStage(order, difficulty));
    }

    // (2026-11-24) 스테이지 이용권: 스테이지 1(모든 난이도)은 무료, 스테이지 2부터는 이용권(인앱 비소모성, 한 번 사면 이후 스테이지 전부)이 있어야 플레이
    // 클리어 해금 조건(ChekcUnlock)은 그대로 → 플레이 가능 = 클리어 해금 + (무료 스테이지 또는 이용권)
    public static bool NeedsStagePass(int order) => order > FIRST_STAGE_ORDER;
    public static bool HasStagePass(UserPurchaseManager purchaseManager)
        => purchaseManager != null && purchaseManager.HasPurchased(InAppProductInfo.STAGE_PASS_KEY);
    public static bool HasStagePass() => HasStagePass(UserDataManager.Instance.userPurchaseManager);
    public bool IsMissingStagePass => NeedsStagePass(order) && !HasStagePass();

    // 해금에 필요한 이전 단계 기록 (스테이지 1 Normal은 null)
    static UserStage GetUnlockPrevStage(int order, DifficultyType difficulty)
    {
        UserStageManager userStageManager = UserDataManager.Instance.userStageManager;
        if (difficulty != DifficultyType.Normal)
            return userStageManager.GetUserStage(order, difficulty - 1);
        if (order > FIRST_STAGE_ORDER)
            return userStageManager.GetUserStage(order - 1, DifficultyType.Normal);
        return null;
    }

    // 잠겨 있을 때 안내 문구 (같은 스테이지 낮은 난이도 / 이전 스테이지 Normal)
    public string GetLockedMessage()
    {
        if (difficulty == DifficultyType.Normal && order > FIRST_STAGE_ORDER)
            return string.Format(TranslateManager.GetText("Stage_Locked"), order); // 이전 스테이지 표시 번호 = (order - 1) + 1
        return string.Format(TranslateManager.GetText("Difficulty_Locked"), UNLOCK_WAVE);
    }

    public static bool IsUnlocked(UserStage lowDifficultyUserStage) => lowDifficultyUserStage != null && lowDifficultyUserStage.isClear;
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

    public static StageData GetStageData(int order, DifficultyType difficultyType)
    {
        return StageManager.Instance != null ? StageManager.Instance.GetStageData(order, difficultyType) : null;
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

        // 에셋 파일명으로 CSV 행을 식별: "{Difficulty}_{order}.asset"(예: Hard_1), 숫자가 없으면 order 0("Normal.asset")
        string assetPath = AssetDatabase.GetAssetPath(this);
        string fileName = Path.GetFileNameWithoutExtension(assetPath);
        if (!TryParseAssetName(fileName, out int expectedOrder, out DifficultyType expectedDifficulty))
        {
            Debug.LogWarning($"[StageData] 에셋 파일명에서 스테이지/난이도 파싱 실패: {assetPath} (예: Hard_1)");
            return;
        }

        string bossName = "";
        foreach (TableReader.Row row in table.rows)
        {
            if (!row.TryGetEnum("Difficulty", out DifficultyType dt) || dt != expectedDifficulty) continue;
            int rowOrder = row.TryGetInt("order", out int so) ? so : FIRST_STAGE_ORDER; // order 열이 없거나 비면 0
            if (rowOrder != expectedOrder) continue;

            order = rowOrder;
            difficulty = dt;
            if (row.TryGetFloat("EnemyHp", out float eh)) enemyHp = eh;
            if (row.TryGetFloat("AddHpRatioPerWave", out float ahr)) addHpRatioPerWave = ahr;
            if (row.TryGetFloat("MultiHpPerWave", out float mh)) multiHpPerWave = mh;
            cycleHpMuls = ParseFloatList(row.Get("CycleHpMuls"), "CycleHpMuls");
            defenceWaveTypes = ParseEnumList<DefenceWaveType>(row.Get("DefenceWaveTypes"), "DefenceWaveTypes");
            bossName = row.Get("BossName");
            break;
        }

        // LoadEventDatas();
        LoadEnemyPrefabs();
        LoadBossPrefab(bossName);
        LoadEnemySpawnerContainerPrefab();
    }

    // "Hard_1" → (1, Hard), "Normal" → (0, Normal)
    public static bool TryParseAssetName(string fileName, out int order, out DifficultyType difficulty)
    {
        order = FIRST_STAGE_ORDER;
        difficulty = DifficultyType.Normal;
        string[] split = fileName.Split('_');
        if (split.Length == 2)
            return System.Enum.TryParse(split[0], out difficulty) && int.TryParse(split[1], out order);
        return System.Enum.TryParse(fileName, out difficulty);
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

    // "1/1.1/1.2" → float 배열 (파싱 실패한 값은 경고 후 제외)
    public float[] ParseFloatList(string raw, string column)
    {
        var list = new List<float>();
        if (string.IsNullOrEmpty(raw))
            return list.ToArray();
        foreach (var s in raw.Split('/'))
        {
            if (float.TryParse(s.Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v)) list.Add(v);
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
    Normal, //중갑 소환
    Armored, //중장갑 소환
    Ethereal, //영체 소환 
    // 4: 중갑형(HeavyArmor) 삭제. 기존 직렬화 값 유지를 위해 번호 비워둠
    // 5: 생체형(Organic) 삭제 (2026-11-07). 기존 직렬화 값 유지를 위해 번호 비워둠
    Light = 6, //경갑 소환 (2026-11-21)
}