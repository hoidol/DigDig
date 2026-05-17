using UnityEngine;

public interface IWayPointerTarget
{
    Transform Transform { get; }
    Sprite Thum { get; }
    float MaxTime { get; }
    float CurTimer { get; }
    void Appear(Vector2 spawnPos);
    void Destroy();

    void ClearArea(Vector2 pos);
}