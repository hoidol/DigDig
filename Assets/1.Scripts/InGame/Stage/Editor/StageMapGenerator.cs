using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// (2026-11-24) 스테이지 맵 프리팹 생성 도구 → Assets/Resources/StageMap/Stage{N}.prefab (게임 시작 때 TileManager가 불러옴)
// Stage0 = 씬에 있던 기존 맵 그대로 프리팹으로 저장. Stage1~5 = 칸 격자(가로 8 × 세로 9, 칸 간격 1.65 = 타일 크기 + 틈)에 글자 그림으로 정의
//   타일 그림: 숫자 = 타일(같은 숫자 = 같은 타일 그룹), 그 외 = 빈칸. 경로: 꺾이는 칸 좌표(x 왼→오 0~7, y 위→아래 0~8), 도착은 모두 같음
//   Tile.index = 칸 좌표 그대로
// 한 번 만든 뒤엔 프리팹을 직접 고치면 됨 (다시 실행하면 Stage1~5 프리팹을 새로 만듦 = 고친 내용 사라짐)
public static class StageMapGenerator
{
    const string PREFAB_FOLDER = "Assets/Resources/StageMap";
    const int GRID_W = 8, GRID_H = 9;
    const float CELL = 1.65f;
    const float SPAWN_SPREAD = 0.175f; // 입구마다 소환 지점 2개 (Stage0처럼 살짝 벌려서 겹치지 않게)

    class MapDef
    {
        public string name;
        public string[] tiles;
        public Vector2Int[][] routes;
    }

    static Vector2Int C(int x, int y) => new(x, y);
    static Vector2 CellPos(Vector2Int c) => new((c.x - (GRID_W - 1) * 0.5f) * CELL, ((GRID_H - 1) * 0.5f - c.y) * CELL);

    static MapDef[] Maps => new[]
    {
        new MapDef
        {
            name = "계단",
            tiles = new[] { "..000...", "...000..", "1...000.", "11...0..", "111...0.", "1111...0", "22222...", "222222..", "........" },
            routes = new[] { new[] { C(0,0), C(1,0), C(1,1), C(2,1), C(2,2), C(3,2), C(3,3), C(4,3), C(4,4), C(5,4), C(5,5), C(6,5), C(6,6), C(7,6), C(7,8), C(0,8) } },
        },
        new MapDef
        {
            name = "양쪽 입구",
            tiles = new[] { "........", ".00.222.", ".00.222.", ".00.222.", "........", ".11.333.", ".11.333.", ".11.333.", "........" },
            routes = new[]
            {
                new[] { C(0,0), C(0,8), C(3,8), C(3,3) },
                new[] { C(7,0), C(7,8), C(4,8), C(3,8), C(3,3) },
            },
        },
        new MapDef
        {
            name = "지그재그",
            tiles = new[] { "........", "0000000.", "........", ".1111111", "........", "2222222.", "........", ".3333333", "........" },
            routes = new[] { new[] { C(0,0), C(7,0), C(7,2), C(0,2), C(0,4), C(7,4), C(7,6), C(0,6), C(0,8), C(7,8) } },
        },
        new MapDef
        {
            name = "달팽이",
            // 바깥을 한 바퀴 돌고 안쪽 섬(가운데 6칸)을 두 바퀴 돈 뒤 (3,2)에서 끝
            tiles = new[] { ".0......", ".0.1111.", ".0....3.", ".0.22.3.", ".0.22.3.", ".0.22.3.", ".0....3.", ".044443.", "........" },
            routes = new[] { new[] { C(0,0), C(0,8), C(7,8), C(7,0), C(2,0), C(2,6), C(5,6), C(5,2), C(2,2), C(2,6), C(5,6), C(5,2), C(3,2) } },
        },
        new MapDef
        {
            name = "세 입구",
            tiles = new[] { ".00.111.", ".00.111.", ".00.111.", "........", "222.3333", "....444.", ".555444.", ".555....", "........" },
            routes = new[]
            {
                new[] { C(0,0), C(0,3), C(3,3), C(3,5), C(0,5), C(0,8), C(7,8) },
                new[] { C(3,0), C(3,3), C(3,5), C(0,5), C(0,8), C(7,8) },
                new[] { C(7,0), C(7,3), C(3,3), C(3,5), C(0,5), C(0,8), C(7,8) },
            },
        },
    };

    [MenuItem("Tools/Stage/Generate Stage Map Prefabs (Stage1~5 다시 만듦)")]
    static void GenerateMenu()
    {
        if (!EditorUtility.DisplayDialog("스테이지 맵 생성", "Resources/StageMap의 Stage1~5 프리팹을 새로 만듭니다. (직접 고친 내용은 사라짐)\n씬에 Stage0이 있으면 Stage0 프리팹으로 저장하고 씬에서 뺍니다.", "생성", "취소"))
            return;
        if (Generate())
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    // 배치모드: Unity -batchmode -executeMethod StageMapGenerator.GenerateBatch
    // -maps 4 또는 -maps 1,3 = 그 맵만 다시 만듦 (없으면 1~5 전부)
    public static void GenerateBatch()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/4.Scenes/InGame.unity", OpenSceneMode.Single);
        int[] only = null;
        string[] args = System.Environment.GetCommandLineArgs();
        int argIdx = System.Array.IndexOf(args, "-maps");
        if (argIdx >= 0 && argIdx + 1 < args.Length)
            only = System.Array.ConvertAll(args[argIdx + 1].Split(','), int.Parse);
        if (!Generate(only))
            throw new System.Exception("StageMapGenerator 실패");
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    public static bool Generate(int[] onlyMapIndices = null)
    {
        TileManager manager = Object.FindFirstObjectByType<TileManager>(FindObjectsInactive.Include);
        EnemyFieldController field = Object.FindFirstObjectByType<EnemyFieldController>(FindObjectsInactive.Include);
        if (manager == null || field == null)
        {
            Debug.LogError("StageMapGenerator: TileManager 또는 EnemyFieldController 없음");
            return false;
        }
        if (!AssetDatabase.IsValidFolder(PREFAB_FOLDER))
            AssetDatabase.CreateFolder("Assets/Resources", "StageMap");

        // Stage0: 씬에 있으면 프리팹으로 저장 후 씬에서 뺌 (없으면 이미 프리팹)
        Transform stage0Tr = manager.transform.Find("Stage0");
        if (stage0Tr != null)
        {
            stage0Tr.gameObject.SetActive(true);
            SetupStage0(stage0Tr, field);
            PrefabUtility.SaveAsPrefabAsset(stage0Tr.gameObject, $"{PREFAB_FOLDER}/Stage0.prefab");
        }
        GameObject stage0Prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_FOLDER}/Stage0.prefab");
        if (stage0Prefab == null)
        {
            Debug.LogError("StageMapGenerator: Stage0 프리팹 없음");
            return false;
        }

        MapDef[] maps = Maps;
        GameObject template = (GameObject)PrefabUtility.InstantiatePrefab(stage0Prefab, manager.transform);
        PrefabUtility.UnpackPrefabInstance(template, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        for (int i = 0; i < maps.Length; i++)
        {
            if (onlyMapIndices != null && System.Array.IndexOf(onlyMapIndices, i + 1) < 0)
                continue;
            GameObject map = BuildMap(template.transform, maps[i], i + 1);
            PrefabUtility.SaveAsPrefabAsset(map, $"{PREFAB_FOLDER}/Stage{i + 1}.prefab");
            Object.DestroyImmediate(map);
        }
        Object.DestroyImmediate(template);

        // 씬 정리: 맵은 게임 시작 때 프리팹으로 불러오므로 씬의 Stage 오브젝트와 예전 경로 참조를 뺌
        for (int c = manager.transform.childCount - 1; c >= 0; c--)
        {
            Transform child = manager.transform.GetChild(c);
            if (child.GetComponent<Stage>() != null || child.name.StartsWith("Stage"))
                Object.DestroyImmediate(child.gameObject);
        }
        field.paths = new Transform[0];
        field.enemySpawnPointTrs = new Transform[0];
        EditorUtility.SetDirty(field);
        Debug.Log("StageMapGenerator: Stage0~5 프리팹 저장 완료");
        return true;
    }

    static void SetupStage0(Transform stage0Tr, EnemyFieldController field)
    {
        Stage stage = stage0Tr.GetComponent<Stage>() ?? stage0Tr.gameObject.AddComponent<Stage>();
        stage.mapIndex = 0;
        stage.mapName = "기본";
        if (stage.tiles == null || stage.tiles.Length == 0)
            stage.CollectTiles();
        if (stage.routes != null && stage.routes.Length > 0)
            return;
        // 예전 방식과 같은 경로: 소환 지점마다 [소환 지점, paths[1] … 끝]
        var routes = new List<StageRoute>();
        if (field.enemySpawnPointTrs != null && field.paths != null && field.paths.Length > 1)
            foreach (Transform spawn in field.enemySpawnPointTrs)
            {
                if (spawn == null)
                    continue;
                var points = new List<Transform> { spawn };
                for (int p = 1; p < field.paths.Length; p++)
                    points.Add(field.paths[p]);
                routes.Add(new StageRoute { points = points.ToArray() });
            }
        stage.routes = routes.ToArray();
    }

    // template = Stage0 복사본 (TileCanvas 설정·타일 모양을 가져옴)
    static GameObject BuildMap(Transform template, MapDef def, int mapIndex)
    {
        var go = new GameObject($"Stage{mapIndex}");
        Transform stageTr = go.transform;
        stageTr.SetParent(template.parent, false);

        // 타일 캔버스: Stage0 캔버스를 복사하고 안을 비움
        Canvas srcCanvas = template.GetComponentInChildren<Canvas>(true);
        Tile tileTemplate = template.GetComponentInChildren<Tile>(true);
        GameObject canvasGo = Object.Instantiate(srcCanvas.gameObject, stageTr);
        canvasGo.name = srcCanvas.name;
        for (int c = canvasGo.transform.childCount - 1; c >= 0; c--)
            Object.DestroyImmediate(canvasGo.transform.GetChild(c).gameObject);
        var tilesRoot = new GameObject("Tiles", typeof(RectTransform)).transform;
        tilesRoot.SetParent(canvasGo.transform, false);

        // 타일 그룹: 숫자마다 하나
        var groups = new SortedDictionary<char, List<Vector2Int>>();
        for (int y = 0; y < def.tiles.Length; y++)
            for (int x = 0; x < def.tiles[y].Length; x++)
            {
                char ch = def.tiles[y][x];
                if (!char.IsDigit(ch))
                    continue;
                if (!groups.TryGetValue(ch, out var list))
                    groups[ch] = list = new List<Vector2Int>();
                list.Add(new Vector2Int(x, y));
            }

        var allTiles = new List<Tile>();
        foreach (var pair in groups)
        {
            var groupGo = new GameObject($"TileGroup{pair.Key}", typeof(RectTransform));
            groupGo.transform.SetParent(tilesRoot, false);
            TileGroup group = groupGo.AddComponent<TileGroup>();
            var groupTiles = new List<Tile>();
            foreach (Vector2Int cell in pair.Value)
            {
                Tile tile = Object.Instantiate(tileTemplate, groupGo.transform);
                tile.name = "Tile";
                var rt = (RectTransform)tile.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.position = stageTr.TransformPoint(CellPos(cell));
                tile.index = cell;
                groupTiles.Add(tile);
            }
            group.tiles = groupTiles.ToArray();
            allTiles.AddRange(groupTiles);

            // 그룹 경로(enemyRoots): 그룹을 둘러싼 한 칸 바깥 사각형 (왼위 → 왼아래 → 오른아래 → 오른위, Stage0과 같은 순서)
            int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
            foreach (Vector2Int c in pair.Value)
            {
                minX = Mathf.Min(minX, c.x); maxX = Mathf.Max(maxX, c.x);
                minY = Mathf.Min(minY, c.y); maxY = Mathf.Max(maxY, c.y);
            }
            int l = Mathf.Max(0, minX - 1), r = Mathf.Min(GRID_W - 1, maxX + 1), t = Mathf.Max(0, minY - 1), b = Mathf.Min(GRID_H - 1, maxY + 1);
            var rootsTr = new GameObject($"TileGroup{pair.Key}Roots").transform;
            rootsTr.SetParent(stageTr, false);
            Vector2Int[] corners = { new(l, t), new(l, b), new(r, b), new(r, t) };
            group.enemyRoots = new Transform[corners.Length];
            for (int k = 0; k < corners.Length; k++)
                group.enemyRoots[k] = CreatePoint(rootsTr, $"Root_{pair.Key}_{k}", CellPos(corners[k]));
        }

        // 적 경로: 입구마다 소환 지점 2개(살짝 벌림), 그 뒤 지점은 같은 칸이면 같은 Transform을 공유 (합쳐지는 곳)
        var routesTr = new GameObject("Routes").transform;
        routesTr.SetParent(stageTr, false);
        var shared = new Dictionary<Vector2Int, Transform>();
        var routes = new List<StageRoute>();
        int pointCount = 0;
        for (int ri = 0; ri < def.routes.Length; ri++)
        {
            Vector2Int[] pts = def.routes[ri];
            var tail = new Transform[pts.Length - 1];
            for (int k = 1; k < pts.Length; k++)
            {
                if (!shared.TryGetValue(pts[k], out Transform tr))
                    shared[pts[k]] = tr = CreatePoint(routesTr, k == pts.Length - 1 ? "Path_End" : $"Path_{pointCount++}", CellPos(pts[k]));
                tail[k - 1] = tr;
            }
            Vector2 start = CellPos(pts[0]);
            Vector2 dir = (CellPos(pts[1]) - start).normalized;
            Vector2 side = new(-dir.y, dir.x);
            for (int s = 0; s < 2; s++)
            {
                var points = new List<Transform> { CreatePoint(routesTr, $"EnemySpawnPoint{ri}_{s}", start + side * (s == 0 ? SPAWN_SPREAD : -SPAWN_SPREAD)) };
                points.AddRange(tail);
                routes.Add(new StageRoute { points = points.ToArray() });
            }
        }

        Stage stage = go.AddComponent<Stage>();
        stage.mapIndex = mapIndex;
        stage.mapName = def.name;
        stage.tiles = allTiles.ToArray();
        stage.routes = routes.ToArray();
        return go;
    }

    static Transform CreatePoint(Transform parent, string name, Vector2 localPos)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false);
        t.localPosition = localPos;
        return t;
    }

    // 맵마다 경로 길이 (난이도 참고용, 콘솔)
    [MenuItem("Tools/Stage/Log Route Lengths")]
    static void LogRouteLengths()
    {
        for (int m = 0; ; m++)
        {
            var stage = AssetDatabase.LoadAssetAtPath<Stage>($"{PREFAB_FOLDER}/Stage{m}.prefab");
            if (stage == null)
                break;
            var lines = new List<string>();
            if (stage.routes != null)
                foreach (StageRoute route in stage.routes)
                {
                    Transform[] p = route.GetValidPoints();
                    float len = 0f;
                    for (int i = 0; p != null && i < p.Length - 1; i++)
                        len += Vector2.Distance(p[i].position, p[i + 1].position);
                    lines.Add(len.ToString("0.0"));
                }
            Debug.Log($"{stage.name} ({stage.mapName}) 타일 {stage.tiles?.Length} · 경로 길이: {string.Join(", ", lines)}");
        }
    }
}
