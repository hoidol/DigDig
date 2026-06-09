using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class SpecialEnemySpawner : MonoBehaviour
{

    public abstract void Spawn();
    public abstract void EndSpawn();
}
