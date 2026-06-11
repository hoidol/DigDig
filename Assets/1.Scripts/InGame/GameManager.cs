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
    protected void Awake()
    {
        GameEventBus.Clear();
        StageData stageDataPrefab = Resources.Load<StageData>($"StageData/{UserManager.stageKey}");
        stageData = Instantiate(stageDataPrefab);
        MapManager.Instance.SpawnMap();
    }

    [field:SerializeField] public float gameTimer
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
    EnemySpawner enemySpawner;
    void StartGame()
    {
        phase = 0;
        gameTimer = 0;
        isPlaying = true;

         enemySpawner = new EnemySpawner();
        StartPhase(phase);

        isClear = false;
        GameEventBus.Publish(new StartGameEvent(stageData));
    }

    public void StartPhase(int phase)
    {
        PhaseData phaseData = stageData.GetPhaseData(phase);
        Debug.Log($"GameManager StartPhase {phase}");
        enemySpawner.StartPattern(phaseData.enemyPatternData);

        if (phaseData.isBoss)
        {
            StartBoss();
        }
        else
        {
            WaitPhase(phaseData.time).Forget();    
        }
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
        GameEventBus.Publish<BossEvent>(new BossEvent());
        Boss boss = Instantiate(stageData.boss);
        
        Vector2Int tileIndex = MapManager.PositionToTileIndex( Player.Instance.transform.position);
        Vector2Int[,] tileIndexArr = MapManager.GetIndexArray(tileIndex, boss.Size);

        boss.Spawn(tileIndexArr);
        BossCanvas.Instance.SetBoss(boss);
    }



    void Update()
    {
        gameTimer += Time.deltaTime;

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.E))
        {

            EnemySpawner spawner = new EnemySpawner();
            spawner.Spawn(EnemyType.Melee);
        }
#endif
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
public class BossEvent
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
