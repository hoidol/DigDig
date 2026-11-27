using UnityEngine;

public interface IWeapon
{
    void Init(Slime player);
    void UpdateWeapon();
    void Attack(Vector2 dir);
    Transform AttackPoint { get; }
    Vector2 AttackDir { get; }

}
