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

    // (2026-11-25) 보스는 풀 밖에서 생성돼 activeEnemies에 없음 (wave 클리어 카운트에서도 빠져 있어야 함) → 대상 찾기용으로만 따로 둠
    readonly HashSet<Boss> activeBosses = new();
    public void RegisterBoss(Boss boss) { if (boss != null) activeBosses.Add(boss); }
    public void UnregisterBoss(Boss boss) { activeBosses.Remove(boss); }

    // 스킬·범위 공격 대상 = 살아 있는 일반 적(엘리트 포함) + 보스. 반환은 복사본 (맞아 죽어도 안전)
    public List<Enemy> GetTargetEnemies()
    {
        var list = new List<Enemy>(activeEnemies.Count + activeBosses.Count);
        foreach (Enemy e in activeEnemies)
            if (e != null && e.CurHp > 0)
                list.Add(e);
        activeBosses.RemoveWhere(b => b == null);
        foreach (Boss b in activeBosses)
            if (b.gameObject.activeInHierarchy && b.CurHp > 0)
                list.Add(b);
        return list;
    }

    public Transform[] enemySpawnPointTrs; // 일반 적 소환 위치 (순서대로 번갈아: 0, 1, 0, 1 …). 엘리트·보스는 paths[0]
    int normalSpawnCount;

    void Start()
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(EnemyDeadEventListener);
        // 브레이크타임 적 경로 표시: 씬에 붙여 두면(값 조정용) 그걸 쓰고, 없으면 기본값으로 붙임
        if (GetComponent<EnemyPathPreview>() == null)
            gameObject.AddComponent<EnemyPathPreview>();
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

    // 테스트 전용: 일반 적 + 보스까지 보상·처치 이벤트 없이 제거
    public void ClearAllForTest()
    {
        ClearAllEnemies();
        foreach (Boss b in new List<Boss>(activeBosses))
        {
            if (b == null)
                continue;
            EnemyWorldCanvas.Instance?.Release(b);
            Destroy(b.gameObject);
        }
        activeBosses.Clear();
    }

    public void EnemyDeadEventListener(EnemyDeadEvent e)
    {
        RemoveActive(e.enemy);
    }

    public int DyingEnemyCount => dyingEnemies.Count;

    #region 경로 (2026-11-24 스테이지 맵마다 경로 여러 개)
    // 경로 = 출발 → 도착 Transform 목록. 출발은 경로마다 다를 수 있고 도착은 모두 같음
    // 실행 중 = TileManager가 고른 스테이지 맵(Stage.routes). Stage에 경로가 없으면 이 컴포넌트의 paths·enemySpawnPointTrs (예전 방식)
    // 일반 적 = 경로를 번갈아 씀, 엘리트·보너스 엘리트·보스 = 주 경로(0번)
    readonly List<Transform[]> routes = new();
    bool routesReady;

    public IReadOnlyList<Transform[]> Routes
    {
        get
        {
            EnsureRoutes();
            return routes;
        }
    }

    public Transform[] MainRoute => Routes.Count > 0 ? routes[0] : paths;

    void EnsureRoutes()
    {
        // 편집 모드(경로 미리보기)는 매번 다시 읽음 (스테이지 켜고 끄기·지점 이동이 바로 반영되게)
        if (routesReady && Application.isPlaying)
            return;
        routesReady = true;
        routes.Clear();

        Stage stage = Application.isPlaying
            ? (TileManager.Instance != null ? TileManager.Instance.ActiveStage : null)
            : TileManager.FindShownStage();
        if (stage != null && stage.routes != null)
            foreach (StageRoute route in stage.routes)
            {
                Transform[] points = route?.GetValidPoints();
                if (points != null && points.Length >= 2)
                    routes.Add(points);
            }
        if (routes.Count > 0)
            return;

        // 예전 방식: 소환 지점마다 [소환 지점, paths[1] … 끝], 소환 지점이 없으면 paths 그대로
        if (paths == null || paths.Length == 0)
            return;
        bool useSpawnPoints = enemySpawnPointTrs != null && enemySpawnPointTrs.Length > 0 && paths.Length > 1;
        if (useSpawnPoints)
            foreach (Transform spawn in enemySpawnPointTrs)
            {
                if (spawn == null)
                    continue;
                var route = new List<Transform> { spawn };
                for (int i = 1; i < paths.Length; i++)
                    if (paths[i] != null)
                        route.Add(paths[i]);
                routes.Add(route.ToArray());
            }
        if (routes.Count == 0)
            routes.Add(System.Array.FindAll(paths, p => p != null));
    }

    // 스테이지 맵이 바뀌면 (TileManager) 다시 읽게
    public void ResetRoutes()
    {
        routesReady = false;
        normalSpawnCount = 0;
    }

    public Vector2 PathStartPosition => MainRoute[0].position;

    // 엘리트·보너스 엘리트·보스 소환 위치 = 주 경로 시작
    public Vector2 GetSpawnPosition(float howFar = -1)
    {
        return PathStartPosition;
    }

    // 일반 적 경로: 0, 1, 2, 0, 1 … 번갈아
    public Transform[] NextNormalRoute()
    {
        IReadOnlyList<Transform[]> list = Routes;
        if (list.Count == 0)
            return paths;
        return list[normalSpawnCount++ % list.Count];
    }

    // 경로 위치 목록 (브레이크타임 경로 표시 EnemyPathPreview가 씀)
    public List<List<Vector2>> GetRoutes()
    {
        var result = new List<List<Vector2>>();
        foreach (Transform[] route in Routes)
        {
            var points = new List<Vector2>();
            foreach (Transform t in route)
                points.Add(t.position);
            result.Add(points);
        }
        return result;
    }
    #endregion
}