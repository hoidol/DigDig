using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
public class GameManager : MonoSingleton<GameManager>
{    
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
    public float breakTimer = 0f; // 휴식 경과 시간 (0 ~ BREAK_TIME)
    public bool isBreak;

    public Wave currentWave { get; private set; }
    WaveTimer CurrentWaveTimer => currentWave is BattleWave battleWave ? battleWave.waveTimer : null;
    // 현재 웨이브 경과 시간 / 전체 시간 (Enemy 체력 증가, 타이머 UI용)
    public float waveTime => CurrentWaveTimer != null ? CurrentWaveTimer.waveTime : 0f;
    public float waveTimer => CurrentWaveTimer != null ? CurrentWaveTimer.waveTime - CurrentWaveTimer.waveTimer : 0f;
    public int phase;
    public DefenceType phaseDefenceType; // 이번 phase에 소환될 적의 방어 타입 (phase 시작 시 DefenceWaveTypes로 결정)
    public int slimeSpawnCount;

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
        GameEventBus.Subscribe<CharacterHpChangedEvent>(OnCharacterHpChangedEvent);
        GameEventBus.Subscribe<SpawnMinieEvent>(OnSpawnMinieEvent);

        Debug.Log($"UserManager.STAGE_KEY {UserDataManager.STAGE_KEY}");
        stageData = StageManager.Instance.GetStageData(UserDataManager.STAGE_KEY);
        stageData.Init();
        enemySpawnerContainer = Instantiate(stageData.enemySpawnerContainerPrefab);
        waves = enemySpawnerContainer.GetComponentsInChildren<Wave>(true);

        // 캐릭터를 임의의 빈 타일 위에 배치
        Tile characterTile = TileManager.Instance.GetRandomEmptyTile();
        if (characterTile != null)
            characterTile.SetTileObject(Character.Instance);
        else
            Debug.LogWarning("GameManager 캐릭터를 올릴 빈 타일 없음");

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

        ProcessWave(phase);
    }

    void ProcessWave(int phase)
    {
        this.phase = phase;
        phaseDefenceType = stageData.GetPhaseDefenceType(phase);
        WaveType waveType = stageData.phaseDatas[phase].waveType;

        GameEventBus.Publish(new PhaseStartEvent(phase));
        // 웨이브 진행/종료는 각 Wave가 담당 (종료 시 Wave.MoveNextWave → EndWave)
        StartWave(phase, waveType);
    }

    public void StartWave(int phase, WaveType waveType = WaveType.Normal)
    {
        isBreak = false;
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
    public void EndWave(int endPhase)
    {
        if (!isPlaying || endPhase != phase || isBreak)
            return;

        if (phase >= GameSetting.BOSS_PHASE)
            return;
        GameEventBus.Publish(new PhaseEndEvent(phase));
        StartBreak(endPhase).Forget();
    }

    // 웨이브 종료 → BREAK_TIME 휴식(SkipBreak로 스킵 가능) → 다음 phase 웨이브 선택
    int breakId; // 부활 시 진행 중인 휴식을 무효화하기 위한 id

    public async UniTaskVoid StartBreak(int endPhase)
    {
        int id = ++breakId;
        isBreak = true;
        breakTimer = 0;
        Debug.Log($"GameManager StartBreak {endPhase}");
        GameEventBus.Publish(new BreakStartEvent(endPhase));

        while (breakTimer < GameSetting.BREAK_TIME)
        {
            await UniTask.Yield();
            if (id != breakId)
                return;
            if (!isPlaying)
                continue;
            breakTimer += Time.deltaTime;
        }

        ProcessWave(endPhase + 1);
    }

    // 전투 웨이브 스킵 (BattleWave.CanSkip 구간에서만)
    public void SkipWave()
    {
        if (currentWave is BattleWave battleWave && !isBreak)
            battleWave.Skip();
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

    // 부활: 필드 정리 후 체력을 가득 채우고 마지막 phase를 처음부터 다시 시작
    // 휴식 중에 죽었으면 직전 phase는 이미 끝났으므로 다음 phase부터
    public void Resume()
    {
        int restartPhase = isBreak ? phase + 1 : phase;

        breakId++; // 진행 중인 휴식 중단
        if (currentWave != null)
            currentWave.StopWave();
        EnemySpawner.Instance.ClearAllEnemies();

        Character.Instance.AddHp(Character.Instance.health.MaxHp);
        isPlaying = true;
        GameEventBus.Publish(new ResumeEvent());

        ProcessWave(restartPhase);
    }

    void OnCharacterHpChangedEvent(CharacterHpChangedEvent e)
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
public class PhaseStartEvent
{
    public int phaseIdx;
    public PhaseStartEvent(int phaseIdx)
    {
        this.phaseIdx = phaseIdx;
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

public class ResumeEvent
{
    
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