using UnityEngine;
public interface IEnemySpecialAttackPattern
{
    Transform Transform { get; }
    float PlayAnim(string animName);
}