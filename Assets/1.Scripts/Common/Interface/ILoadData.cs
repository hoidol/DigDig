using Cysharp.Threading.Tasks;
using UnityEngine;

public interface ILoadData
{
    UniTask LoadTask { get; }
}
