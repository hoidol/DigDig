using UnityEngine;
using Cysharp.Threading.Tasks;

public interface IComboAttackItem
{
    UniTask OnAttack(Player player, Vector2 dir);
}
