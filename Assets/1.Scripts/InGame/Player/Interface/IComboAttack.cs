using UnityEngine;
using Cysharp.Threading.Tasks;

public interface IComboFire
{
    UniTask OnComboFire(Vector2 dir);
}
