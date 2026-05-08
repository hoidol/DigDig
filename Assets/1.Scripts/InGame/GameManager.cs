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
    public int underground = 0;
    public int wave;
    public int destroyOreStone { get; private set; }
    public StageData stageData;

    public List<IGameListener> gameListeners = new List<IGameListener>();

    public void AddGameListener(IGameListener op)
    {
        if (!gameListeners.Contains(op))
            gameListeners.Add(op);
    }
    public void RemoveGameListener(IGameListener op)
    {
        if (gameListeners.Contains(op))
            gameListeners.Remove(op);
    }
    public bool isClear;
    protected void Awake()
    {
        GameEventBus.Clear();
        stageData = Resources.Load<StageData>($"StageData/{User.Instance.stageKey}");
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
    //아이템 선택 -> 파밍 -> 2분 뒤 전투 (난이도 :약) -> 1분 뒤 특정 위치 도착 유도(3분 안에 특정 위치 도착 유도)



    public bool isPlaying;
    void StargGame()
    {
        isPlaying = false;
        SelectItemCanvas.Instance.OpenCanvas(Grade.Normal,
        () =>
        {
            isPlaying = true;
        });


        isClear = false;
        underground = 0;
        GameEventBus.Publish(new StartGameEvent(stageData));
        StartUnderground(underground);
    }

    public void StartUnderground(int lv)
    {
        underground = lv;
        wave = 0;
        UndergroundData undergroundData = GetUndergroundData();
        if (undergroundData.isBoss)
        {
            StartBoss();
        }
        else
        {
            WaitWave(wave);
        }

        GameEventBus.Publish(new UndergroundStartEvent(undergroundData));
    }

    void StartBoss()
    {
        GameEventBus.Publish<BossEvent>(new BossEvent());
        gameState = GameState.Boss;
    }

    void WaitWave(int nextWave)
    {
        gameState = GameState.WaitingWave;
        StartCoroutine(CoWaitingWave(nextWave));
        StartCoroutine(CoSpawn()); //적이 조금씩만 생성
    }

    public float waveWaitingTimer = 0;
    public bool waving;
    IEnumerator CoWaitingWave(int nextWave)
    {
        waveWaitingTimer = 0;
        float waveWaitTime = StageData.WAIT_WAVE_TIMES[GetUndergroundData().idx];
        while (true)
        {
            if (waveWaitingTimer >= waveWaitTime)
            {
                //웨이브 시작
                StartWave(nextWave);
                yield break;
            }
            yield return null;
            waveWaitingTimer += Time.deltaTime;
        }
    }

    async UniTaskVoid StartWave(int w)
    {
        gameState = GameState.Wave;
        wave = w;
        waving = true;
        WaveData waveData = GetWaveData();
        GameEventBus.Publish(new WaveStartEvent(waveData)); // WaveStartEvent() 객체 발행


        EnemyPatternData enemyPatternData = EnemyManager.Instance.GetEnemyPattern(waveData.patternType);
        EnemyPattern enemyPattern = Instantiate(enemyPatternData.enemyPatternPrefab);
        enemyPattern.StartPattern();

        int mSec = (int)(StageData.WAVE_TIMES[GetUndergroundData().idx] * 1000);
        await UniTask.Delay(mSec);

        enemyPattern.EndPattern();
        EndWave();
    }

    void EndWave()
    {
        waving = false;
        WaveData wData = GetWaveData(); //현재 웨이브
        if (wData.idx < GetUndergroundData().waveDatas.Length - 1)
        {
            WaitWave(wave + 1);
        }
        else
        {
            Debug.Log("모든 웨이브 끝!");
            EndUnderground();
        }
        GameEventBus.Publish(new WaveEndEvent(GetWaveData()));

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
            yield return new WaitForSeconds(Random.Range(6, 8));
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

    public void EndUnderground()
    {
        isClear = true;
        gameState = GameState.ClearUnderground;
        //플레이어가 Enterance로 들어감
        if (underground < stageData.undergroundDatas.Length)
        {
            EndUndergroundEffect().Forget();
        }

        GameEventBus.Publish(new UndergroundEndEvent(GetUndergroundData()));
    }
    async UniTaskVoid EndUndergroundEffect()
    {
        CameraManager.Instance.Shake(3f, 3f);
        await UniTask.Delay(1000);
        FadeCanvs.Instance.FadeOutIn($"더 깊은 곳으로\n지하 {underground + 1}층", () =>
        {
            StartUnderground(underground + 1);
        });
    }

    public UndergroundData GetUndergroundData()
    {
        return stageData.undergroundDatas[underground];
    }
    public WaveData GetWaveData()
    {
        return GetUndergroundData().waveDatas[wave];
    }

    void EnemyDeadEventListener(EnemyDeadEvent e)
    {
        Gold.Dropped(e.position, "0");
    }
}

public interface IGameListener
{
    void StartUnderground(UndergroundData uData);
    void EndUnderground();
    void StartWave(WaveData wData);
    void EndWave();
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
    WaitingWave,
    Wave,
    ClearUnderground,
    Boss
}
public class BossEvent
{

}