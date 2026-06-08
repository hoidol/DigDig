using UnityEngine;

public interface IWeapon
{
    void Init(Player player);
    void UpdateWeapon();
    void Attack(Vector2 dir);
    Transform AttackPoint { get; }
    Vector2 LastAttackDir { get; }

}
