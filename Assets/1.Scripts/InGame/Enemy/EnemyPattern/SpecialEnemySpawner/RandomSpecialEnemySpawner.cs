using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
public class RandomSpecialEnemySpawner : SpecialEnemySpawner
{
    public int includeSize;
    public int excludeSize;

    [SerializeField] EnemySpawnCountChance[] enemySpawnCountChances; //참고용 - Chance 총합이 100이 안될 수도 있음, 한번에 Count이상 소환 못함 
    EnemySpawnCountChance[] currentEnemySpawnState;//확률 및 개수 카운딩용

    public override void Spawn()
    {
        // Debug.Log("RandomSpecialEnemySpawner Spawn()!!@!");
        //List<Vector2Int> indies = new List<Vector2Int>();
        //임시의 한점
        List<Vector2Int> indies = MapManager.GetEmptyTileIndexesInRange(Character.Instance.transform.position, includeSize, excludeSize);

        currentEnemySpawnState = System.Array.ConvertAll(enemySpawnCountChances, e => new EnemySpawnCountChance
        {
            enemyType = e.enemyType,
            chance = e.chance,
            count = e.count
        });


        // // 랜덤 배치를 위해 indies 셔플
        for (int i = indies.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (indies[i], indies[r]) = (indies[r], indies[i]);
        }

        Vector2Int[,] spawnTileArray = null;
        foreach (var idx in indies)
        {
            float roll = Random.Range(0f, 100f);
            float cumulative = 0f;
            // Debug.Log($"RandomSpecialEnemySpawner 랜덤 roll {roll}");
            foreach (var state in currentEnemySpawnState)
            {
                if (state.count <= 0) continue;  // 소환 한도 초과 스킵
                cumulative += state.chance;
                if (roll < cumulative)
                {

                    // Debug.Log($"RandomSpecialEnemySpawner 적 소환해야됌  {state.enemyType}");
                    if (MapManager.GetTileArray(idx, EnemyManager.GetEnemyData(state.enemyType).size, out spawnTileArray))
                    {
                        Enemy enemyPrefab = GameManager.Instance.stageData.GetEnemyPrefab(state.enemyType);
                        Enemy enemy = EnemyManager.Instance.Instantiate(enemyPrefab);
                        enemy?.Spawn(spawnTileArray);
                        state.count--;
                    }

                    break;
                }
            }
            // roll이 총 chance 합계 초과 → 해당 타일엔 소환 안 함
        }
    }



    public override void EndSpawn()
    {

    }


}
