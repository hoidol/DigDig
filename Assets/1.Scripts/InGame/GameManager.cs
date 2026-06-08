using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
public class GameManager : MonoSingleton<GameManager>
{
    public int phase;
    // public int underground = 0;
    // public int wave;
    public int destroyOreStone { get; private set; }
    public StageData stageData;

    public bool isClear;
    protected void Awake()
    {
        GameEventBus.Clear();
        StageData stageDataPrefab = Resources.Load<StageData>($"StageData/{UserManager.stageKey}");
        stageData = Instantiate(stageDataPrefab);
        MapManager.Instance.SpawnMap();
    }

    void Start()
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(EnemyDeadEventListener);

        FadeCanvs.Instance.FadeIn("망각의 늪", () =>
        {
            StartGame();


        });

        // Joystick joystick = FindFirstObjectByType<Joystick>();
        // joystick.gameObject.SetActive(false);
        // Player.Instance.gameObject.SetActive(false);
        // PlayerInLobby.Instance.StartGame(() =>
        // {
        //     Player.Instance.gameObject.SetActive(true);
        //     joystick.gameObject.SetActive(true);
        //     
        //     StartUnderground(1);
        // });
    }

    //게임 흐름
    // 아이템 선택 
    // -> Ordeal 3갈래 시작
    // -> 


    public bool isPlaying;
    void StartGame()
    {
        phase = 0;
        isPlaying = true;
        StartPhase(phase);

        isClear = false;
        GameEventBus.Publish(new StartGameEvent(stageData));
    }

    public void StartPhase(int phase)
    {
        PhaseData phaseData = stageData.GetPhaseData(phase);
        Debug.Log($"GameManager StartPhase {phase}");
        EnemyPatternSpawner spawner = new EnemyPatternSpawner();
        spawner.StartPattern(phaseData.enemyPatternData);

        if (phaseData.isBoss)
        {
            StartBoss();
        }



    }




    public void EndPhase()
    {
        phase++;
        StartPhase(phase);
    }

    void StartBoss()
    {
        GameEventBus.Publish<BossEvent>(new BossEvent());
    }


#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {

            EnemyPatternSpawner spawner = new EnemyPatternSpawner();
            spawner.Spawn(EnemyType.Melee);
        }
    }
#endif



    public void AddDestroyOreStone(int amount = 1)
    {
        destroyOreStone += amount;

        // 필요하면 여기서 UI 업데이트, 세이브, 업적 체크 등도 같이 처리
    }


    public void EndGame(bool clear)
    {
        string msg = clear ? "승리" : "패배";
        FadeCanvs.Instance.FadeIn($"msg", () => { SceneManager.LoadScene("InGame"); });
    }

    void EnemyDeadEventListener(EnemyDeadEvent e)
    {
        //Gold.Dropped(e.position);
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
