using UnityEngine;

public interface IHpUI
{
    float MaxHp { get; }
    float CurHp { get; }
    Vector3 HpUIPosition { get; }
}
