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
    public GameState gameState
    {
        get;
        private set;
    }
    public int ordealClearCount; //9~12개? 해보자
    // public int underground = 0;
    // public int wave;
    public int destroyOreStone { get; private set; }
    public StageData stageData;

    public bool isClear;
    protected void Awake()
    {
        GameEventBus.Clear();
        stageData = Resources.Load<StageData>($"StageData/{User.Instance.stageKey}");
        MapManager.Instance.SpawnMap();
    }

    void Start()
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(EnemyDeadEventListener);

        FadeCanvs.Instance.FadeIn("망각의 늪", () =>
        {
            StargGame();


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
    void StargGame()
    {
        isPlaying = false;
        SelectItemCanvas.Instance.OpenCanvas(Grade.Normal,
        () =>
        {
            isPlaying = true;
            StartCoroutine(CoSpawn());
            StartOrdeal();

        });

        isClear = false;
        GameEventBus.Publish(new StartGameEvent(stageData));
    }

    public void StartOrdeal()
    {
        gameState = GameState.UndergoingOrdeal;
        OrdealProgressData ordealProgressData = stageData.GetOrdealProgressData();
        if (ordealProgressData.isBoss)
        {
            StartBoss();
            return;
        }
        else
        {
            OrdealManager.Instance.StartOrdeal(ordealProgressData);
        }

    }

    public void EndOrdeal(OrdealData ordealData)
    {
        ordealClearCount++;
        //여기서 특수 이벤트 실행 처리!
        gameState = GameState.ClearOrdeal;
        GameEventBus.Publish(new OrdealEndEvent(ordealData, ordealClearCount));

        //10초 뒤에 시작하기
        Invoke(nameof(StartOrdeal), 10f);
    }

    void StartBoss()
    {
        GameEventBus.Publish<BossEvent>(new BossEvent());
        gameState = GameState.Boss;
    }


#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Spawn(EnemyType.Melee);
        }
    }
#endif


    public Enemy Spawn(EnemyType type)
    {
        Enemy enemy = EnemyManager.Instance.GetEnemy(type);
        Vector2 randomPos = (Vector2)Player.Instance.transform.position + Random.insideUnitCircle.normalized * Random.Range(15f, 17f);
        enemy.Spawn(randomPos);
        return enemy;
    }


    public void AddDestroyOreStone(int amount = 1)
    {
        destroyOreStone += amount;

        // 필요하면 여기서 UI 업데이트, 세이브, 업적 체크 등도 같이 처리
    }


    IEnumerator CoSpawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(6, 10));

            if (gameState == GameState.UndergoingOrdeal)
                continue;

            Enemy enemy = EnemyManager.Instance.GetEnemy(EnemyType.Melee);
            Vector2 randomPos = (Vector2)Player.Instance.transform.position + Random.insideUnitCircle.normalized * 15;
            enemy.Spawn(randomPos);
        }
    }

    public void EndGame(bool clear)
    {
        string msg = clear ? "승리" : "패배";
        FadeCanvs.Instance.FadeIn($"msg", () => { SceneManager.LoadScene("InGame"); });
    }

    void EnemyDeadEventListener(EnemyDeadEvent e)
    {
        Gold.Dropped(e.position, "0");
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
public enum GameState
{
    //ApproachingOrdeal, //시련 다가가는중
    UndergoingOrdeal, //시련 중
    ClearOrdeal, //시련 종료
    Boss
}
public class BossEvent
{

}