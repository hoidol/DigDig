using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using DG.Tweening;
public class GameManager : MonoSingleton<GameManager>
{
    public int underground = 1;
    public int wave;
    public int destroyOreStone { get; private set; }
    public StageData stageData;
    public UndergroundEnterance undergroundEnterance;

    public List<IGameListener> gameListeners = new List<IGameListener>();
    public bool isClear;
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
    void Start()
    {
        underground = 1;
        wave = 1;

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
        stageData = Resources.Load<StageData>($"StageData/{User.Instance.stageKey}");


        StartUnderground(1);
    }
    public void StartUnderground(int lv)
    {
        isClear = false;
        underground = lv;
        wave = 1;

        for (int i = 0; i < gameListeners.Count; i++)
        {
            gameListeners[i].StartUnderground(GetUndergroundData());
        }
        StartCoroutine(CWaitingWave());
        StartCoroutine(CoSpawn());
    }
    public float waveWaitingTimer = 0;
    public bool waving;
    IEnumerator CWaitingWave()
    {
        waveWaitingTimer = 0;
        float waveWaitTime = GetUndergroundData().waveWaitTime;
        while (true)
        {
            if (waveWaitingTimer >= waveWaitTime)
            {
                //웨이브 시작
                StartWave();
                yield break;
            }
            yield return null;
            waveWaitingTimer += Time.deltaTime;
        }
    }
    public UndergroundData GetUndergroundData()
    {
        return stageData.undergroundDatas[underground - 1];
    }
    public WaveData GetWaveData()
    {
        return GetUndergroundData().waveDatas[wave - 1];
    }
    Dictionary<EnemyType, int> enemyCounterDic = new Dictionary<EnemyType, int>();
    List<Coroutine> spawnEnemyCor = new List<Coroutine>();
    void StartWave()
    {
        waving = true; //30
        WaveData waveData = GetWaveData();
        UndergroundData undergroundData = GetUndergroundData();
        spawnEnemyCor.Clear();
        enemyCounterDic.Clear();
        for (int i = 0; i < gameListeners.Count; i++)
        {
            gameListeners[i].StartWave(waveData);
        }
        for (int i = 0; i < waveData.enemySpawnDatas.Length; i++)
        {
            StartCoroutine(CoSpawnEnemy(waveData.enemySpawnDatas[i]));
        }

        StartCoroutine(CoWave(waveData.waveTime));
    }

    IEnumerator CoWave(float waveTime)
    {
        yield return new WaitForSeconds(waveTime);
        EndWave();
    }
    void EndWave()
    {
        waving = false;
        for (int i = 0; i < spawnEnemyCor.Count; i++)
        {
            StopCoroutine(spawnEnemyCor[i]);
        }
    }
    IEnumerator CoSpawnEnemy(EnemySpawnData spawnData)
    {
        while (true)
        {
            float wait = Random.Range(spawnData.intervalRange.x, spawnData.intervalRange.y);
            yield return new WaitForSeconds(wait);
            int count = Random.Range(spawnData.countRange.x, spawnData.countRange.y);
            for (int i = 0; i < count; i++)
            {
                if (!enemyCounterDic.ContainsKey(spawnData.enemyType))
                {
                    enemyCounterDic.Add(spawnData.enemyType, 0);
                }
                if (enemyCounterDic[spawnData.enemyType] >= spawnData.maxCount)
                {
                    continue;
                }
                Enemy enemy = EnemyManager.Instance.GetEnemy(spawnData.enemyType);
                enemyCounterDic[spawnData.enemyType]++;
                Vector2 randomPos = (Vector2)Player.Instance.transform.position + Random.insideUnitCircle.normalized * Random.Range(15f, 17f);
                enemy.Spawn(randomPos);
            }
        }
    }


    public void AddDestroyOreStone(int amount = 1)
    {
        destroyOreStone += amount;

        // 필요하면 여기서 UI 업데이트, 세이브, 업적 체크 등도 같이 처리
        // UpdateDestroyOreStoneUI();
    }


    IEnumerator CoSpawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(13);
            Enemy enemy = EnemyManager.Instance.GetEnemy(EnemyType.Ranged);
            // float length = (MagmaCore.Instance.transform.position - Player.Instance.transform.position).magnitude + Camera.main.orthographicSize * 2;
            Vector2 randomPos = (Vector2)Player.Instance.transform.position + Random.insideUnitCircle.normalized * 15;

            enemy.Spawn(randomPos);

        }
    }

    public void EndGame(bool clear)
    {

    }

    public void EndUnderground()
    {
        isClear = true;
        //플레이어가 Enterance로 들어감
        Player.Instance.transform.DOScale(0, 0.7f);
        Player.Instance.transform.DOMove(undergroundEnterance.transform.position, 0.6f).OnComplete(() =>
        {
            //화면 전환

        });

    }
}

public interface IGameListener
{
    void StartUnderground(UndergroundData uData);
    void EndUnderground();
    void StartWave(WaveData wData);
    void EndWave();
}