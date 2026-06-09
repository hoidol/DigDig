using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
public class StraightSpecialEnemySpawner : SpecialEnemySpawner
{ 
    public int includeSize;
    public int excludeSize;  
    [SerializeField] EnemySpawnCountChance[] enemySpawnCountChances; //참고용 - Chance 총합이 100이 안될 수도 있음, 한번에 Count이상 소환 못함 
    EnemySpawnCountChance[] currentEnemySpawnState;//확률 및 개수 카운딩용
    
    public override void Spawn()
    {
        List<Vector2Int> indies = new List<Vector2Int>();
        //임시의 한점
        
        Vector2 dirToPlayer = Vector2.zero - (Vector2)Player.Instance.transform.position;
        Vector2 dir = Vector2.zero;
        dir.x = dirToPlayer.x > 0? 1 : -1;
        dir.y = dirToPlayer.y > 0? 1 : -1;
        Vector2Int oppDir = new Vector2Int(-(int)dir.x,-(int)dir.y);


        //대각선 찾으려고
        dir = dir.normalized * Mathf.Sqrt(Mathf.Pow(MapManager.TILE_SIZE,2) +  Mathf.Pow(MapManager.TILE_SIZE,2));
        
        currentEnemySpawnState = System.Array.ConvertAll(enemySpawnCountChances, e => new EnemySpawnCountChance
        {
            key = e.key,
            chance = e.chance,
            count = e.count
        });
        
        Vector2Int startIndex = MapManager.PositionToIndex((Vector2)Player.Instance.transform.position + dir * includeSize);
        Vector2Int tempIndex = startIndex;
        for(int i = 0; i < includeSize; i++)
        {
            
            for(int j = 0; j < includeSize; j++)
            {
                if(MapManager.CheckEmpty(tempIndex))
                    indies.Add(tempIndex);

                tempIndex.y += oppDir.y;    
            }
            tempIndex.x += oppDir.x;
            tempIndex.y = startIndex.y;
        }

        // 랜덤 배치를 위해 indies 셔플
        for (int i = indies.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (indies[i], indies[r]) = (indies[r], indies[i]);
        }

        foreach (var idx in indies)
        {
            float roll = Random.Range(0f, 100f);
            float cumulative = 0f;

            foreach (var state in currentEnemySpawnState)
            {
                if (state.count <= 0) continue;  // 소환 한도 초과 스킵
                cumulative += state.chance;
                if (roll < cumulative)
                {
                    // EnemyManager.Spawn(state.key, MapManager.IndexToPosition(idx));
                    state.count--;
                    break;
                }
            }
            // roll이 총 chance 합계 초과 → 해당 타일엔 소환 안 함
        }
    }

    public class EnemySpawnCountChance
    {
        public string key;
        public float chance; //30 이면 30% 
        public int count;
    }

    public override void EndSpawn()
    {
        
    }


}
