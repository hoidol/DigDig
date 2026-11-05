using UnityEngine;

public interface IWeapon
{
    void Init(KingSlime player);
    void UpdateWeapon();
    void Attack(Vector2 dir);
    Transform AttackPoint { get; }
    Vector2 AttackDir { get; }

}
