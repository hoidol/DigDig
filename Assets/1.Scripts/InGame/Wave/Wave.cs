using UnityEngine;

public abstract class Wave : MonoBehaviour
{
    public WaveType waveType;

    public abstract void StartWave(int phase);

    // 다음 phase로 넘어가지 않고 웨이브만 중지 (부활 시 같은 phase 재시작용)
    public virtual void StopWave()
    {
    }
    public virtual void EndWave()
    {
        MoveNextWave();
    }

    public virtual void MoveNextWave()
    {
        Debug.Log($"Wave MoveNextWave() currrent {waveType}");
        GameManager.Instance.EndWave(GameManager.Instance.phase);
    }
}
public enum WaveType
{
    Normal,
    Hard,
    Elite,
    Boss
}
