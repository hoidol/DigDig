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
    public int phase;
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
    async void Start()
    {
        await UniTask.WhenAll(
            StageManager.Instance.LoadTask,
            StatManager.Instance.LoadTask,
            BulletManager.Instance.LoadTask,
            ItemManager.Instance.LoadTask,
            EnemyManager.Instance.LoadTask
        );

        GameEventBus.Subscribe<EnemyDeadEvent>(EnemyDeadEventListener);
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Subscribe<BossDeadEvent>(OnBossDeadEvent);

        GameEventBus.Subscribe<PlayerHpChangedEvent>(OnPlayerHpChangedEvent);

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
        phase = 0;
        gameTimer = 0;
        isPlaying = true;


        GameEventBus.Publish(new StartGameEvent(stageData));

        StartPhase(phase);
    }
    PhaseData phaseData;
    public void StartPhase(int phase)
    {
        // phase = stageData.phaseDatas.Length - 1; //보스 테스트용 - 테스트 후 주석하기
        phaseData = stageData.GetPhaseData(phase);
        Debug.Log($"GameManager StartPhase {phase}");
        if (phaseData.isBoss)
        {
            StartBoss();
        }

        WaitPhase(phaseData.time).Forget();
        GameEventBus.Publish(new PhaseStartEvent(phase));
    }

    async UniTaskVoid WaitPhase(float t)
    {
        float elapsed = 0f;
        while (elapsed < t)
        {
            await UniTask.Yield();

            if (!isPlaying)
                continue;

            elapsed += Time.deltaTime;
        }
        EndPhase();
    }

    public void EndPhase()
    {
        if (phaseData.isBoss)
            return;

        phase++;

        StartPhase(phase);
    }

    void StartBoss()
    {
        enemySpawnerContainer.bossSpawner.Spawn();
    }


    void Update()
    {
        gameTimer += Time.deltaTime;
    }

    void OnBossDeadEvent(BossDeadEvent e)
    {
        if (Player.Instance.curHp <= 0)
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

    public void Recure()
    {
        Player.Instance.AddHp(Player.Instance.health.MaxHp);
        isPlaying = true;
    }
    void OnPlayerHpChangedEvent(PlayerHpChangedEvent e)
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


public class PhaseStartEvent
{
    public int phaseIdx;

    public PhaseStartEvent(int p)
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
