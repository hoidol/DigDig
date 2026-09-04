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
    public float dayTimer = 0f; // 낮 40초 <-> 밤 80초
    public float nightTimer = 0f;
    public bool isDay;
    public int phase;
    public int slimeSpawnCount;
    async void Start()
    {
        await UniTask.WhenAll(
            StageManager.Instance.LoadTask,
            BulletManager.Instance.LoadTask,
            ItemManager.Instance.LoadTask,
            EnemyManager.Instance.LoadTask
        );

        GameEventBus.Subscribe<EnemyDeadEvent>(EnemyDeadEventListener);
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Subscribe<BossDeadEvent>(OnBossDeadEvent);
        GameEventBus.Subscribe<CharacterHpChangedEvent>(OnPlayerHpChangedEvent);
        GameEventBus.Subscribe<SpawnMinieEvent>(OnSpawnMinieEvent);

        Debug.Log($"UserManager.STAGE_KEY {UserManager.STAGE_KEY}");
        stageData = StageManager.Instance.GetStageData(UserManager.STAGE_KEY);
        stageData.Init();
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


        ProcessDay(phase).Forget();
    }

    public PhaseData phaseData;
    public void StartDay(int phase)
    {
        isDay = true;
        // phase = stageData.phaseDatas.Length - 1; //보스 테스트용 - 테스트 후 주석하기
        Debug.Log($"GameManager StartPhase {phase}");
        GameEventBus.Publish(new DayStartEvent(phase));
    }
    public void StartNight(int phase)
    {
        isDay = false;
        // phase = stageData.phaseDatas.Length - 1; //보스 테스트용 - 테스트 후 주석하기
        Debug.Log($"GameManager StartNight {phase}");

        GameEventBus.Publish(new NightStartEvent(phase));
        if (phaseData.isBoss)
        {
            StartBoss();
        }
    }

    async UniTaskVoid ProcessDay(int phase)
    {
        phaseData = stageData.GetPhaseData(phase);

        GameEventBus.Publish(new PhaseStartEvent(phaseData));
        //낮에 대한 시간 처리
        dayTimer = 0;
        StartDay(phase);
        float dayTime = GameSetting.DAY_TIME + GameSetting.DAY_INCREASE_TIME * phase;
        if (dayTime >= GameSetting.MIX_DAY_TIME)
        {
            dayTime = GameSetting.MIX_DAY_TIME;
        }
        Debug.Log($"day {phase} dayTime {dayTime}");
        while (dayTimer <= dayTime)
        {
            await UniTask.Yield();

            if (!isPlaying)
                continue;

            dayTimer += Time.deltaTime;
        }

        //밤에 대한 시간 처리
        StartNight(phase);
        nightTimer = 0;

        float nightTime = GameSetting.NIGHT_TIME + GameSetting.NIGHT_INCREASE_TIME * phase;
        if (nightTime >= GameSetting.MIX_NIGHT_TIME)
        {
            nightTime = GameSetting.MIX_NIGHT_TIME;
        }

        Debug.Log($"day {phase} nightTime {nightTime}");
        while (nightTimer <= nightTime)
        {
            await UniTask.Yield();

            if (!isPlaying)
                continue;

            nightTimer += Time.deltaTime;
        }

        EndAllDay();
    }

    public void EndAllDay()
    {
        if (phaseData.isBoss)
            return;

        GameEventBus.Publish(new PhaseEndEvent(phase));
        phase++;
        ProcessDay(phase).Forget();
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
        if (Character.Instance.curHp <= 0)
            return;
        EndGame(true);
    }

    const string ACCEPT_REVIEW_KEY = "ACCEPT_REVIEW_KEY";

    public void EndGame(bool clear)
    {
        if (!isPlaying)
            return;

        isPlaying = false;
        UserManager.Instance.userStageManager.TryStage(stageData.key);
        if (!clear)
        {
            FailCanvas.Instance.OpenCanvas();
        }
        else
        {
            int accept = PlayerPrefs.GetInt(ACCEPT_REVIEW_KEY, 0);
            if (accept == 0)
            {
                StageData maxStageData = StageManager.Instance.GetStageData(UserManager.Instance.userStageManager.GetMaxStage());
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


public class DayStartEvent
{
    public int phaseIdx;

    public DayStartEvent(int p)
    {
        phaseIdx = p;
    }
}
public class NightStartEvent
{
    public int phaseIdx;

    public NightStartEvent(int p)
    {
        phaseIdx = p;
    }
}

public class PhaseStartEvent
{
    public PhaseData phaseData;
    public PhaseStartEvent(PhaseData pData)
    {
        phaseData = pData;
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

/*
성장2이 있어야되는 이유
뱀성에 각성같은거야... 필요해

//명,암,화(마법),수(유연 합성),목(성장,회복),금(물리, 공격),토(방어)
Growth2 EnhanceStone로 슬라임

*/