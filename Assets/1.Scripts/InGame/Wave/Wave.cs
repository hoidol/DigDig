using UnityEngine;

public abstract class Wave : MonoBehaviour
{
    public WaveType waveType;
    public int waveIdx;
    public DefenceType waveDefenceType;
    public bool isEnd;
    public bool isSpawnEnd; // 이 wave의 소환이 모두 끝남 → 다음 wave 바로 시작 가능

    public virtual void StartWave(int wIdx, DefenceType defenceType)
    {
        waveIdx = wIdx;
        isEnd = false;
        isSpawnEnd = false;
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

        // 카드 소환 모드: 시간으로 끝나지 않음 (적을 다 처리하면 GameManager가 종료)
        if (GameSetting.USE_WAVE_MINION_OFFER)
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
    Battle,
    Elite,
    Boss
}
