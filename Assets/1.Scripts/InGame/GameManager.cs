using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
public class GameManager : MonoSingleton<GameManager>
{    
    public int destroyStoneCount { get; private set; }
    public int killEnemyCount { get; private set; }
    public StageData stageData;

    public bool isPlaying;
    public Wave[] waves;
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
    public bool isBreak;

    public Wave currentWave { get; private set; }
    WaveTimer CurrentWaveTimer => currentWave is BattleWave battleWave ? battleWave.waveTimer : null;
    // 현재 웨이브 경과 시간 / 전체 시간 (Enemy 체력 증가, 타이머 UI용)
    public float waveTime => CurrentWaveTimer != null ? CurrentWaveTimer.waveTime : 0f;
    public float waveTimer => CurrentWaveTimer != null ? CurrentWaveTimer.waveTime - CurrentWaveTimer.waveTimer : 0f;
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
        GameEventBus.Subscribe<CharacterHpChangedEvent>(OnPlayerHpChangedEvent);
        GameEventBus.Subscribe<SpawnMinieEvent>(OnSpawnMinieEvent);

        Debug.Log($"UserManager.STAGE_KEY {UserDataManager.STAGE_KEY}");
        stageData = StageManager.Instance.GetStageData(UserDataManager.STAGE_KEY);
        stageData.Init();
        enemySpawnerContainer = Instantiate(stageData.enemySpawnerContainerPrefab);
        waves = enemySpawnerContainer.GetComponentsInChildren<Wave>(true);
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

    async UniTaskVoid ProcessWave(int phase)
    {
        this.phase = phase;
        var waveType = WaveType.Normal;
        var waveWayDirection = WaveWayDirection.Top;

        if(phase != 0)
        {
            (waveType, waveWayDirection) = await SelectWaveCanvas.Instance.OpenCanvas(stageData.phaseDatas[phase]);
        }

        GameEventBus.Publish(new PhaseStartEvent(phase, stageData.phaseDatas[phase].enemySpawnPatternDatas));
        // 웨이브 진행/종료는 각 Wave가 담당 (종료 시 Wave.MoveNextWave → EndWave)
        StartWave(phase, waveType);
    }

    public void StartWave(int phase, WaveType waveType = WaveType.Normal)
    {
        isBreak = false;
        // phase = GameSetting.BOSS_PHASE; //보스 테스트용 - 테스트 후 주석하기
        Debug.Log($"GameManager StartWave {phase}");
        currentWave = GetWave(waveType);
        if (currentWave == null)
        {
            Debug.LogError($"GameManager {waveType} Wave 없음 → Normal로 대체");
            waveType = WaveType.Normal;
            currentWave = GetWave(waveType);
        }
        currentWave.StartWave(phase);
        GameEventBus.Publish(new WaveStartEvent(phase,waveType));
    }

    // Wave.MoveNextWave에서 호출 → 다음 phase 웨이브 선택으로
    public void EndWave(int endPhase, WaveType waveType = WaveType.Normal)
    {
        if (!isPlaying || endPhase != phase)
            return;

        if (phase >= GameSetting.BOSS_PHASE)
            return;
        StartBreak(endPhase).Forget();
        GameEventBus.Publish(new PhaseEndEvent(phase));
    }

    public async UniTaskVoid StartBreak(int endPhase)
    {
        isBreak = true;

        Debug.Log($"GameManager StartPhase {phase}");
        

        endPhase++;
        ProcessWave(endPhase).Forget();
        GameEventBus.Publish(new BreakStartEvent(phase));
    }
    
    Wave GetWave(WaveType waveType)
    {
        return waves.Where(e=>e.waveType == waveType).FirstOrDefault();
    }

    public void SkipBreak()
    {
        if (!isBreak)
            return;

        breakTimer = GameSetting.BREAK_TIME;
    }



    void Update()
    {
        if (!isPlaying)
            return;

        gameTimer += Time.deltaTime;
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
    public WaveType waveType;
    public WaveStartEvent(int p,WaveType waveType)
    {
        phaseIdx = p;
        this.waveType = waveType;
    }
}
public class WaveEndEvent
{
    public int phaseIdx;
    public WaveType waveType;
    public WaveEndEvent(int p,WaveType waveType)
    {
        phaseIdx = p;
        this.waveType = waveType;
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