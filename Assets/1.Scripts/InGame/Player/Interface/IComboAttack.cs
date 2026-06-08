using UnityEngine;
using Cysharp.Threading.Tasks;

public interface IComboAttack
{
    UniTask OnAttack(Player player, Vector2 dir);
}
