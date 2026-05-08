using UnityEngine;

public interface IWayPointerTarget
{
    Transform Transform { get; }
    Sprite Thum { get; }
    float MaxTime { get; }
    float CurTimer { get; }
}