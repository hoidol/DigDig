using System;
using System.Collections.Generic;
using LayerLab.ArtMakerUnity;
using UnityEngine;

public class EnemySpawner : MonoSingleton<EnemySpawner>
{
    readonly Dictionary<EnemyType, Stack<Enemy>> pool = new(); // 적 종류 별 풀링
    public Transform[] paths;
    [field: SerializeField] public int ActiveEnemyCount { get; private set; }

    public readonly HashSet<Enemy> activeEnemies = new();

    public EnemyArea enemyAreaPrefab;
    public readonly List<EnemyArea> enemyAreas = new();

    void Start()
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(EnemyDeadEventListener);
        GameEventBus.Subscribe<MapRoomCreatedEvent>(OnMapRoomCreatedEvent);
    }

    // 방이 생성될 때마다 중심에 EnemyArea 배치
    void OnMapRoomCreatedEvent(MapRoomCreatedEvent e)
    {
        if (enemyAreaPrefab == null)
        {
            Debug.LogWarning("EnemySpawner enemyAreaPrefab 미등록");
            return;
        }

        EnemyArea enemyArea = GameObject.Instantiate(enemyAreaPrefab, e.room.CenterPosition, Quaternion.identity);
        enemyArea.room = e.room;
        enemyAreas.Add(enemyArea);
    }

    public Enemy Instantiate(Enemy prefab)
    {
        // EnemyData data = EnemyManager.enemyDataDic[prefab.enemyType];

        if (!pool.ContainsKey(prefab.enemyType))
            pool[prefab.enemyType] = new Stack<Enemy>();

        Enemy enemy = pool[prefab.enemyType].Count > 0
            ? pool[prefab.enemyType].Pop()
            : GameObject.Instantiate(prefab);


        enemy.gameObject.SetActive(true);
        activeEnemies.Add(enemy);
        ActiveEnemyCount = activeEnemies.Count;
        return enemy;
    }

    void ReleaseEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);

        // 이미 회수된 적(EnemyDeadEvent 중복)이나 스포너 밖에서 생성된 적(보스)은 풀/카운트에 반영하지 않음
        // → 같은 적이 풀에 두 번 들어가 카운트가 0이 안 되는 문제 방지
        if (!activeEnemies.Remove(enemy))
            return;

        if (!pool.ContainsKey(enemy.enemyType))
            pool[enemy.enemyType] = new Stack<Enemy>();
        pool[enemy.enemyType].Push(enemy);
        ActiveEnemyCount = activeEnemies.Count;
    }

    // 필드의 모든 적을 처치 이벤트/보상 없이 회수 (부활 시)
    public void ClearAllEnemies()
    {
        foreach (Enemy enemy in new List<Enemy>(activeEnemies))
            ReleaseEnemy(enemy);
    }

    public void EnemyDeadEventListener(EnemyDeadEvent e)
    {
        ReleaseEnemy(e.enemy);
    }
    public Vector2 PathStartPosition => paths[0].position;

    public Vector2 GetSpawnPosition(float howFar = -1)
    {
        return PathStartPosition;
    }

}