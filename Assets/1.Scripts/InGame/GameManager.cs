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
    public int phase;
    public int destroyOreCount { get; private set; }
    public int killEnemyCount { get; private set; }
    public StageData stageData;

    public bool isClear;
    public bool isPlaying;
    EnemySpawnerContainer enemySpawnerContainer;
    protected void Awake()
    {
        GameEventBus.Clear();
        Debug.Log($"UserManager.STAGE_KEY {UserManager.STAGE_KEY}");
        stageData = StageManager.Instance.GetStageData(UserManager.STAGE_KEY);

        stageData.Init();
        enemySpawnerContainer = Instantiate(stageData.enemySpawnerContainerPrefab);

        MapManager.Instance.SpawnMap();
    }

    [field: SerializeField]
    public float gameTimer
    {
        get;
        private set;
    }
    void Start()
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(EnemyDeadEventListener);
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);

        Joystick joystick = FindFirstObjectByType<Joystick>();
        joystick.gameObject.SetActive(false);
        FadeCanvs.Instance.FadeIn(stageData.Title, () =>
        {
            joystick.gameObject.SetActive(true);
            StartGame();
        });
    }
    void StartGame()
    {
        phase = 0;
        gameTimer = 0;
        isPlaying = true;


        isClear = false;
        GameEventBus.Publish(new StartGameEvent(stageData));

        StartPhase(phase);
    }

    public void StartPhase(int phase)
    {
        phase = stageData.phaseDatas.Length - 1; //보스 테스트용 - 테스트 후 주석하기
        PhaseData phaseData = stageData.GetPhaseData(phase);
        Debug.Log($"GameManager StartPhase {phase}");
        if (phaseData.isBoss)
        {
            StartBoss();
        }
        else
        {
            WaitPhase(phaseData.time).Forget();
        }



        GameEventBus.Publish(new PhaseStartEvent(phase));
    }

    async UniTaskVoid WaitPhase(float t)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(t));
        EndPhase();
    }

    public void EndPhase()
    {
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


    public void EndGame(bool clear)
    {
        string msg = clear ? "승리" : "패배";
        FadeCanvs.Instance.FadeIn($"msg", () => { SceneManager.LoadScene("InGame"); });
    }

    void EnemyDeadEventListener(EnemyDeadEvent e)
    {
        killEnemyCount++;
    }
    public void OnDestroyedStoneEvent(DestroyedStoneEvent e)
    {
        destroyOreCount++;

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
