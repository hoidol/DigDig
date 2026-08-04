using UnityEditor.Rendering;
using UnityEngine;

public class FillEnemyPattern : SpawnPattern
{
    //빈공간 많으면 적 추가 생산
    int spawnCount = 0;
    public float checkTime;
    float checkTimer;
    public int limitRoomCount = 45;//여유 공간
    public SpecialEnemySpawner specialEnemySpawner;

    public override void StartGame()
    {
        spawnCount = 0;
    }

    public void Update()
    {
        if (!GameManager.Instance.isPlaying)
            return;

        if (checkTimer >= checkTime)
        {
            int emptyCount = 0;
            for (int x = 0; x < MapManager.tileArray.GetLength(0); x++)
            {
                for (int y = 0; y < MapManager.tileArray.GetLength(1); y++)
                {
                    Vector2 pos = MapManager.TileIndexToPosition(new Vector2Int(x, y));
                    if (pos.magnitude > Player.Instance.distanceMaxDistanceDestroiedStone)
                    {
                        continue;
                    }
                    if (MapManager.tileArray[x, y] == null)
                    {
                        emptyCount++;
                    }
                }
            }
            Debug.Log($"FillEnemyPattern Whole emptyCount {emptyCount}");
            if (emptyCount >= limitRoomCount)
            {
                specialEnemySpawner.Spawn();
            }
            checkTimer = 0;
        }
        checkTimer += Time.deltaTime;
    }
}