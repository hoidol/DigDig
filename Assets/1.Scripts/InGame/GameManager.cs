using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class GameManager : MonoSingleton<GameManager>
{
    List<ILoadData> loadDatas = new List<ILoadData>();
    // public int phase;
    public int destroyStoneCount { get; private set; }
    public int killEnemyCount { get; private set; }
    public StageData stageData;

    public bool isPlaying;
    EnemySpawnerContainer enemySpawnerContainer;
    protected void Awake()
    {
        GameEventBus.Clear();
    }

    [field: SerializeField]
    public float gameTimer
    {
        get;
        private set;
    }
    public float breakTimer = 0f; // 낮 40초 <-> 밤 80초
    public float waveTimer = 0f;
    public float waveTime = 0f;
    public bool isBreak;
    public int phase;
    public int slimeSpawnCount;
    public DifficultyType difficultyType;
    
    async void Start()
    {
        await UniTask.WhenAll(
            StageManager.Instance.LoadTask,
            BulletManager.Instance.LoadTask,
            ItemManager.Instance.LoadTask,
            EnemyManager.Instance.LoadTask,
            EquipmentManager.Instance.LoadTask,
            SlimeManager.Instance.LoadTask
        );

        GameEventBus.Subscribe<EnemyDeadEvent>(EnemyDeadEventListener);
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Subscribe<BossDeadEvent>(OnBossDeadEvent);
        GameEventBus.Subscribe<CharacterHpChangedEvent>(OnPlayerHpChangedEvent);
        GameEventBus.Subscribe<SpawnMinieEvent>(OnSpawnMinieEvent);

        Debug.Log($"UserManager.STAGE_KEY {UserDataManager.STAGE_KEY}");
        stageData = StageManager.Instance.GetStageData(UserDataManager.STAGE_KEY);
        stageData.Init();
        phaseData = stageData.phaseData;
        enemySpawnerContainer = Instantiate(stageData.enemySpawnerContainerPrefab);

        GameEventBus.Publish(new StartGameEvent(stageData));
        // 기존 Start() 로직 (이벤트 구독, FadeIn → StartGame) 이어서 진행
        FadeCanvs.Instance.FadeIn(stageData.Title, () =>
        {
            StartGame();
        });
    }

    void StartGame()
    {
        phase = 0;
        gameTimer = 0;
        isPlaying = true;


        ProcessWave(phase).Forget();
    }

    public PhaseData phaseData;
    public void StartBreak(int phase)
    {
        isBreak = true;
        // phase = GameSetting.BOSS_PHASE; //보스 테스트용 - 테스트 후 주석하기
        Debug.Log($"GameManager StartPhase {phase}");
        GameEventBus.Publish(new BreakStartEvent(phase));
    }
    public void StartWave(int phase)
    {
        isBreak = false;
        // phase = GameSetting.BOSS_PHASE; //보스 테스트용 - 테스트 후 주석하기
        Debug.Log($"GameManager StartWave {phase}");

        GameEventBus.Publish(new WaveStartEvent(phase));
        if (phase == GameSetting.BOSS_PHASE)
        {
            StartBoss();
        }
    }

    async UniTaskVoid ProcessWave(int phase)
    {
        if (phase < GameSetting.BOSS_PHASE)
        {
            GameEventBus.Publish(new PhaseStartEvent(phase, stageData.phaseData.enemyPatternData[phase].enemySpawnPatternDatas));
        }
        //낮에 대한 시간 처리
        breakTimer = 0;
        StartBreak(phase);
        float breakTime = GameSetting.BREAK_TIME;
        while (breakTimer <= breakTime)
        {
            await UniTask.Yield();

            if (!isPlaying)
                continue;

            breakTimer += Time.deltaTime;
        }

        //밤에 대한 시간 처리
        StartWave(phase);
        waveTimer = 0;

        waveTime = GameSetting.WAVE_TIME + GameSetting.WAVE_INCREASE_TIME * phase;
        if (waveTime >= GameSetting.MIX_WAVE_TIME)
        {
            waveTime = GameSetting.MIX_WAVE_TIME;
        }

        Debug.Log($"day {phase} waveTime {waveTime}");
        while (waveTimer <= waveTime)
        {
            await UniTask.Yield();

            if (!isPlaying)
                continue;

            waveTimer += Time.deltaTime;
        }

        EndWave();
    }

    public void EndWave()
    {
        if (phase == GameSetting.BOSS_PHASE)
            return;

        GameEventBus.Publish(new PhaseEndEvent(phase));
        phase++;
        ProcessWave(phase).Forget();
    }

    void StartBoss()
    {
        enemySpawnerContainer.bossSpawner.Spawn();
    }


    void Update()
    {
        if (!isPlaying)
            return;

        gameTimer += Time.deltaTime;
    }

    void OnBossDeadEvent(BossDeadEvent e)
    {
        if (Character.Instance.CurHp <= 0)
            return;
        EndGame(true);
    }

    const string ACCEPT_REVIEW_KEY = "ACCEPT_REVIEW_KEY";

    public void EndGame(bool clear)
    {
        if (!isPlaying)
            return;

        isPlaying = false;
        UserDataManager.Instance.userStageManager.TryStage(stageData.key);
        GameEventBus.Publish(new TryStageEvent(stageData.key));

        if (!clear)
        {
            FailCanvas.Instance.OpenCanvas();
        }
        else
        {
            GameEventBus.Publish(new ClearStageEvent(stageData.key));
            int accept = PlayerPrefs.GetInt(ACCEPT_REVIEW_KEY, 0);
            if (accept == 0)
            {
                StageData maxStageData = StageManager.Instance.GetStageData(UserDataManager.Instance.userStageManager.GetMaxStage(UserDataManager.difficultyType));
                if (maxStageData.order >= 2)
                {
                    YesOrNoCanvas.Instance.OpenCanvas(TranslateManager.GetText("review_title"), TranslateManager.GetText("review_body"), (accept) =>
                    {
                        if (accept)
                        {
                            Review review = new Review();
                            review.Request();

                            PlayerPrefs.SetInt(ACCEPT_REVIEW_KEY, 1);
                        }
                    });

                }
            }
            ResultCanvas.Instance.OpenCanvas(true);
        }

        // string msg = clear ? "승리" : "패배";
        // FadeCanvs.Instance.FadeIn($"msg", () => { SceneManager.LoadScene("InGame"); });
    }

    public void Resume()
    {
        Character.Instance.AddHp(Character.Instance.health.MaxHp);
        isPlaying = true;
    }

    void OnPlayerHpChangedEvent(CharacterHpChangedEvent e)
    {
        if (e.curHp <= 0)
        {
            EndGame(false);
        }
    }

    void EnemyDeadEventListener(EnemyDeadEvent e)
    {
        killEnemyCount++;
    }
    public void OnDestroyedStoneEvent(DestroyedStoneEvent e)
    {
        destroyStoneCount++;

        // 필요하면 여기서 UI 업데이트, 세이브, 업적 체크 등도 같이 처리
    }
    public void OnSpawnMinieEvent(SpawnMinieEvent e)
    {
        slimeSpawnCount++;
    }

}


public class StartGameEvent
{
    public StageData stageData;
    public StartGameEvent(StageData data)
    {
        stageData = data;
    }
}
public class StartBossEvent
{

}


public class BreakStartEvent
{
    public int phaseIdx;

    public BreakStartEvent(int p)
    {
        phaseIdx = p;
    }
}
public class WaveStartEvent
{
    public int phaseIdx;

    public WaveStartEvent(int p)
    {
        phaseIdx = p;
    }
}

public class PhaseStartEvent
{
    public int phaseIdx;
    public EnemySpawnPatternData[] wavePatternDatas;
    public PhaseStartEvent(int phaseIdx, EnemySpawnPatternData[] wavePatternDatas)
    {
        this.phaseIdx = phaseIdx;
        this.wavePatternDatas = wavePatternDatas;
    }
}
public class PhaseEndEvent
{
    public int phaseIdx;
    public PhaseEndEvent(int p)
    {
        phaseIdx = p;
    }
}
public class ClearStageEvent
{
    public string key;
    public ClearStageEvent(string key)
    {   
        this.key = key;
    }
}


public class TryStageEvent
{
    public string key;
    public TryStageEvent(string key)
    {   
        this.key = key;
    }
}
/*
성장2이 있어야되는 이유
뱀서에 각성같은거야... 필요해
그냥 되는게 있고 안되는게 있게 하자 마음 편하게 
모든 얘가 될 수는 없어
양분 슬라임은 없을 수 있는거야 
조커 슬라임도 없을 수 있는거야

- 불량 + 화염 : 폭주 슬라임 
- 라이트닝 + [?] : 토르 슬라임 : 
- 탄성 + 관통 : 새총 슬라임  : 화면 전체로 팅기는 
- 불량 : 궤도 슬라임 커지고 많아지고 
- 화염 얼음 : 
- 폭발 


불량
탄성
화염
궤도
관통

얼음

썬더
라이트닝

도파민
치료
폭발
방패 - 


*/