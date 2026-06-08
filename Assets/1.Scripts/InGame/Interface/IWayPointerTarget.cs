using UnityEngine;

public interface IWayPointerTarget
{
    Transform Transform { get; }
    Sprite GetThum();
    float MaxTime { get; }
    float CurTimer { get; }
    void Appear(Vector2 spawnPos);
    void Destroy();

    void ClearArea(Vector2 pos);
}