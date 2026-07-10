using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class StopBossBehaviour : BossBehaviour
{
    public StopBossBehaviour()
    {
        behaviourName = "Stop";
    }
    public Vector2 stopTimeRange;
    public async override UniTask StartBehaviour()
    {
        //이동 X
        await UniTask.Delay(TimeSpan.FromSeconds(UnityEngine.Random.Range(stopTimeRange.x,stopTimeRange.y)));
    }

}