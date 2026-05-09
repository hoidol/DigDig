using UnityEngine;

public interface IWeapon
{
    void Init(Player player);
    void UpdateWeapon();
    void Attack(Vector2 dir, bool fromPlayer);
    Transform AttackPoint { get; }
    Vector2 LastAttackDir { get; }

}
