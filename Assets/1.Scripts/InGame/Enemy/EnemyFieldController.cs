using System;
using System.Collections.Generic;
using LayerLab.ArtMakerUnity;
using UnityEngine;

public class EnemyFieldController : MonoSingleton<EnemyFieldController>
{
    readonly Dictionary<(EnemyType, DefenceType), Stack<Enemy>> pool = new(); // 적 종류 + 방어 타입 별 풀링 (방어 타입마다 프리팹이 다름)
    public Transform[] paths;
    [field: SerializeField] public int ActiveEnemyCount { get; private set; }

    public readonly HashSet<Enemy> activeEnemies = new();
    // 죽어서 activeEnemies에서는 빠졌지만 Dead 애니메이션 중이라 아직 풀에 안 들어간 적
    readonly HashSet<Enemy> dyingEnemies = new();

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
            Debug.LogWarning("EnemyFieldController enemyAreaPrefab 미등록");
            return;
        }

        EnemyArea enemyArea = GameObject.Instantiate(enemyAreaPrefab, e.room.CenterPosition, Quaternion.identity);
        enemyArea.room = e.room;
        enemyAreas.Add(enemyArea);
    }

    public Enemy Instantiate(Enemy prefab)
    {
        // EnemyData data = EnemyManager.enemyDataDic[prefab.enemyType];
        if (prefab == null)
        {
            Debug.Log("EnemySPawner Instantiate() if(prefab == null)");
        }
        var key = (prefab.enemyType, prefab.defenceType);
        if (!pool.ContainsKey(key))
            pool[key] = new Stack<Enemy>();

        Enemy enemy = pool[key].Count > 0
            ? pool[key].Pop()
            : GameObject.Instantiate(prefab);

        // 활성화는 Enemy.Spawn에서 위치를 정한 뒤에 함 → 풀에서 꺼낸 적이 이전 위치에 잠깐 보이지 않게
        activeEnemies.Add(enemy);
        ActiveEnemyCount = activeEnemies.Count;
        return enemy;
    }

    // 이미 회수된 적(EnemyDeadEvent 중복)이나 스포너 밖에서 생성된 적(보스)은 풀/카운트에 반영하지 않음
    // → 같은 적이 풀에 두 번 들어가 카운트가 0이 안 되는 문제 방지
    // 죽은 적은 바로 카운트에서 빼고(dyingEnemies), 비활성화될 때(Enemy.Deactivate → ReturnToPool) 풀에 넣음
    void RemoveActive(Enemy enemy)
    {
        if (!activeEnemies.Remove(enemy))
            return;
        dyingEnemies.Add(enemy);
        ActiveEnemyCount = activeEnemies.Count;
    }

    public void ReturnToPool(Enemy enemy)
    {
        RemoveActive(enemy);
        if (!dyingEnemies.Remove(enemy))
            return;

        var key = (enemy.enemyType, enemy.defenceType);
        if (!pool.ContainsKey(key))
            pool[key] = new Stack<Enemy>();
        pool[key].Push(enemy);
    }

    // 필드의 모든 적(Dead 애니메이션 중 포함)을 처치 이벤트/보상 없이 회수 (부활 시)
    public void ClearAllEnemies()
    {
        foreach (Enemy enemy in new List<Enemy>(activeEnemies))
            enemy.Deactivate();
        foreach (Enemy enemy in new List<Enemy>(dyingEnemies))
            enemy.Deactivate();
    }

    public void EnemyDeadEventListener(EnemyDeadEvent e)
    {
        RemoveActive(e.enemy);
    }

    public int DyingEnemyCount => dyingEnemies.Count;
    public Vector2 PathStartPosition => paths[0].position;

    public Vector2 GetSpawnPosition(float howFar = -1)
    {
        return PathStartPosition;
    }

}