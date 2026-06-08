using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class SpecialEnemyPatternSpawner : MonoBehaviour
{

    public abstract void Spawn();
}
