using Cysharp.Threading.Tasks;
using UnityEngine;

public class Ordeal : MonoBehaviour
{
    public OrdealType ordealType;
    OrdealData ordealData;
    public EnemyPattern enemyPattern;
    public virtual void StartOrdeal(OrdealData ordealData)
    {
        gameObject.SetActive(true);
        if (enemyPattern == null)
            enemyPattern = GetComponent<EnemyPattern>();
        this.ordealData = ordealData;
        enemyPattern.StartPattern(ordealData.enemyPatternData);
    }

    public virtual void EndOrdeal()
    {
        GameManager.Instance.EndOrdeal(ordealData);
    }

    // async UniTaskVoid StartWave(int w)
    // {
    //     gameState = GameState.Wave;
    //     wave = w;
    //     waving = true;
    //     WaveData waveData = GetWaveData();
    //     GameEventBus.Publish(new WaveStartEvent(waveData)); // WaveStartEvent() 객체 발행


    //     EnemyPatternData enemyPatternData = EnemyManager.Instance.GetEnemyPattern(waveData.patternType);
    //     EnemyPattern enemyPattern = Instantiate(enemyPatternData.enemyPatternPrefab);
    //     enemyPattern.StartPattern();

    //     int mSec = (int)(StageData.WAVE_TIMES[GetUndergroundData().idx] * 1000);
    //     await UniTask.Delay(mSec);

    //     enemyPattern.EndPattern();
    //     EndWave();
    // }
}

