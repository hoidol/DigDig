using UnityEngine;

public class WaveWarningCanvas : CanvasUI<WaveWarningCanvas>
{
    // public Transform directionTr;
    // public Transform surroundTr;
    public float showTime = 4f; // 경고 표시 시간(초)
    int surroundPointerCount = 12;
    float surroundPointerSpeed = 1.5f;

    public void StartWaveWarning(WaveWarningType waveWarningType, Vector2 direction)
    {
        gameObject.SetActive(true);
        // directionTr.gameObject.SetActive(false);
        // surroundTr.gameObject.SetActive(false);

        Debug.Log($"WaveWarningCanvas StartWaveWarning waveWarningType:{waveWarningType} direction: {direction}");
        switch (waveWarningType)
        {
            case WaveWarningType.Direction:
                // directionTr.gameObject.SetActive(true);
                // directionTr.right = -direction;
                Vector2 pos = Vector2.zero + (direction * (CameraManager.Instance.mainCamera.orthographicSize + 5f));
                for(int i = 0; i < 5; i++)
                {
                    WaveWarningPointer pointer = WaveWarningPointer.Instantiate(pos);                    
                    pointer.Spawn(pos +Random.insideUnitCircle, -direction,0.6f);
                }
                
                break;
            case WaveWarningType.Surround:
                // surroundTr.gameObject.SetActive(true);
                float radius = CameraManager.Instance.mainCamera.orthographicSize + 5f;
                for (int i = 0; i < surroundPointerCount; i++)
                {
                    float angle = 360f / surroundPointerCount * i * Mathf.Deg2Rad;
                    Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    Vector2 spawnPos = Vector2.zero + dir * radius;
                    WaveWarningPointer.Instantiate(spawnPos).Spawn(spawnPos, -dir, surroundPointerSpeed);
                }
                break;
        }

        CancelInvoke(nameof(HideWaveWarning));
        Invoke(nameof(HideWaveWarning), showTime);
    }

    void HideWaveWarning()
    {
        gameObject.SetActive(false);
    }
}

public enum WaveWarningType
{
    Surround, Direction
}