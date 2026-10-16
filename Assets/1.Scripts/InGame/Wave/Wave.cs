using UnityEngine;

public abstract class Wave : MonoBehaviour
{
    public WaveType waveType;

    // void Start()
    // {
    //     GameEventBus.Subscribe<WaveStartEvent>(OnWaveStartEvent);
    // }
    
    // void OnWaveStartEvent(WaveStartEvent e)
    // {
    //     if(e.waveType == waveType)
    //     {
    //         StartWave();
    //     }
    // }

    public abstract void StartWave(int phase);
    public virtual  void EndWave()
    {
        MoveNextWave();
    }

    public virtual void MoveNextWave()
    {
        Debug.Log($"Wave MoveNextWave() currrent {waveType}");
        
        GameEventBus.Publish(new WaveEndEvent(GameManager.Instance.phase, waveType));
        GameManager.Instance.EndWave(GameManager.Instance.phase, waveType);
    }
}
public enum WaveType
{
    Normal,
    Hard,
    Break, //체력 회복, 가끔 스네이크 나옴(체력 풀피 대신 + 패널티)
    Break_Snake, //체력 회복, 가끔 스네이크 나옴(체력 풀피 대신 + 패널티)
    Shop, //
    Elite,
    MidBoss,
    Boss
}