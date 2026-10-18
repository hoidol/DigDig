using UnityEngine;

public abstract class Wave : MonoBehaviour
{
    public WaveType waveType;
    public int waveIdx;
    public DefenceType waveDefenceType;
    public bool isEnd;

    public virtual void StartWave(int wIdx, DefenceType defenceType)
    {
        waveIdx = wIdx;
        isEnd = false;
        waveDefenceType = defenceType;
        waveTimer = GameSetting.WAVE_TIME;
    }

    public float waveTimer;
    void Update()
    {
        if (isEnd)
            return;

        if (!GameManager.Instance.isPlaying)
            return;

        if (waveTimer <= 0)
        {
            EndWave();
            return;
        }

        waveTimer -= Time.deltaTime;
    }

    public virtual void EndWave()
    {
        Debug.Log($"Wave EndWave {waveIdx}");
        isEnd = true;
        gameObject.SetActive(false);
        GameManager.Instance.EndWave(waveIdx);
    }

}
public enum WaveType
{
    Normal,
    Hard,
    Elite,
    Boss
}
