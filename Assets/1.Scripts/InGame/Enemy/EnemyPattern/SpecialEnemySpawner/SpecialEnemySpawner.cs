using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class SpecialEnemySpawner : MonoBehaviour
{
    //조건에 따라서 Spawn() 실행하기


    public abstract void Spawn();
    public abstract void EndSpawn();
}


[System.Serializable]
public class EnemySpawnCountChance
{
    public EnemyType enemyType;
    public float chance; //30 이면 30% 
    public int count;
}
