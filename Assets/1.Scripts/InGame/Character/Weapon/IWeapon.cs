using UnityEngine;

public interface IWeapon
{
    void Init(Character player);
    void UpdateWeapon();
    void Attack(Vector2 dir);
    Transform AttackPoint { get; }
    Vector2 LastDir { get; }

}
