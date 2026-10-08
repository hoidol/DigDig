using UnityEngine;

public interface IWayPointerTarget
{
    Transform Transform { get; }
    Sprite GetThum();
    void Appear(Vector2 spawnPos);
    void Destroy();
}