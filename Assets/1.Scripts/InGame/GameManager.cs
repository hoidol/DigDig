using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System;
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
    public int day;
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

        Debug.Log($"UserManager.STAGE_KEY {UserManager.STAGE_KEY}");
        stageData = StageManager.Instance.GetStageData(UserManager.STAGE_KEY);
        stageData.Init();
        enemySpawnerContainer = Instantiate(stageData.enemySpawnerContainerPrefab);
        MapManager.Instance.SpawnMap();
        // 기존 Start() 로직 (이벤트 구독, FadeIn → StartGame) 이어서 진행
        FadeCanvs.Instance.FadeIn(stageData.Title, () =>
        {
            StartGame();
        });
    }

    void StartGame()
    {
        day = 0;
        gameTimer = 0;
        isPlaying = true;

        GameEventBus.Publish(new StartGameEvent(stageData));


        ProcessDay(day).Forget();
    }

    public PhaseData phaseData;
    public void StartDay(int day)
    {
        isDay= true;
        // phase = stageData.phaseDatas.Length - 1; //보스 테스트용 - 테스트 후 주석하기
        Debug.Log($"GameManager StartPhase {day}");
        GameEventBus.Publish(new DayStartEvent(day));
    }
    public void StartNight(int day)
    {
        isDay= false;
        // phase = stageData.phaseDatas.Length - 1; //보스 테스트용 - 테스트 후 주석하기
        Debug.Log($"GameManager StartNight {day}");
        if (phaseData.isBoss)
        {
            StartBoss();
        }
    }

    async UniTaskVoid ProcessDay(int day)
    {
        phaseData = stageData.GetPhaseData(day);

        //낮에 대한 시간 처리
        dayTimer = 0;
        StartDay(day);
        float dayTime = GameSetting.DAY_TIME + GameSetting.DAY_INCREASE_TIME * day;
        if(dayTime >= GameSetting.MIX_DAY_TIME)
        {
            dayTime = GameSetting.MIX_DAY_TIME;
        }
        while (dayTimer >= dayTime)
        {
            await UniTask.Yield();

            if (!isPlaying)
                continue;

            dayTimer += Time.deltaTime;
        }

        //밤에 대한 시간 처리
        StartNight(day);
        nightTimer = 0;

        float nightTime = GameSetting.NIGHT_TIME + GameSetting.NIGHT_INCREASE_TIME * day;
        if(nightTime >= GameSetting.MIX_NIGHT_TIME)
        {
            nightTime = GameSetting.MIX_NIGHT_TIME;
        }

        while (nightTimer >= nightTime)
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

        day++;
        // Time.timeScale = 0;
        // SelectItemCanvas.Instance.OpenCanvas(() =>
        // {
        //     Time.timeScale = 1;
        // });
        // StartPhase(phase);
        ProcessDay(day).Forget();
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
    public void EndGame(bool clear)
    {
        if (!isPlaying)
            return;

        isPlaying = false;
        if (!clear)
        {
            FailCanvas.Instance.OpenCanvas();
        }
        else
        {
            ResultCanvas.Instance.OpenCanvas();
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

public class PhaseEndEvent
{
    public int phaseIdx;
    public PhaseEndEvent(int p)
    {
        phaseIdx = p;
    }
}

/*
광석을 빨리 파밍하고 싶게 만들어야돼
[정비] -> 버튼을 누르면 레벨업을 하던가 
파밍 -> 디펜스 게임이야
파밍 + 강화 시스템 어떤식으로 진행하면 좋을까...
뭘 파밍하고 뭘 만들까
어떻게 해야지 재밌게 강화 능력을 얻게 될까...

5초씩 늘려 10
30초 -> 35초 
1분씩 주는거야
3개의 광석 
[강화]
광석 10개 + 구매 횟수*2 -> [?]살 수 있음

[조합]

강화/조합

*/