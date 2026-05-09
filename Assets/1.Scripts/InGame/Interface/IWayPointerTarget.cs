using UnityEngine;

public interface IWayPointerTarget
{
    Transform Transform { get; }
    Sprite Thum { get; }
    float MaxTime { get; }
    float CurTimer { get; }
    void OnAppear(Vector2 spawnPos);
    void OnDestroy();

    void ClearArea(Vector2 pos);
}