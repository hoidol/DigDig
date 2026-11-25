using System.Collections.Generic;
using UnityEngine;

// (2026-11-24) 스테이지 맵 하나 (TileManager 하위 Stage0, Stage1 …): 타일·적 경로·소환 지점이 이 오브젝트 아래에 있음
// 실행 중엔 TileManager가 mapIndex로 하나만 켬 (스테이지 order와 같은 번호, 없으면 0번)
// Tile.index: 인스펙터 버튼으로 위치 기준 자동 설정(임시) → 각 Tile 인스펙터나 이 컴포넌트 목록에서 직접 수정
[System.Serializable]
public class StageRoute
{
    public Transform[] points; // 출발 → 도착. 모든 경로의 도착은 같은 위치, 갈래는 같은 Transform(지점)에서 합쳐짐

    public Transform[] GetValidPoints() => points == null ? null : System.Array.FindAll(points, p => p != null);
}

public class Stage : MonoBehaviour
{
    public int mapIndex; // 스테이지 order (0 = 스테이지 1)
    public string mapName; // 메모용 이름 (예: 달팽이)
    public Tile[] tiles; // 이 스테이지의 타일 ('타일 모으기' 버튼 = 하위 전부)
    // 적 경로. 0번 = 주 경로(엘리트·보스), 일반 적은 번갈아. 비우면 EnemyFieldController의 paths·소환 지점(예전 방식)
    public StageRoute[] routes;

    public void CollectTiles()
    {
        tiles = GetComponentsInChildren<Tile>(true);
    }

    // 위치 기준: x 왼→오 0부터, y 맨 윗줄 0부터 아래로 (TileManager와 같은 규칙)
    public void AssignTileIndicesByPosition()
    {
        if (tiles == null || tiles.Length == 0)
            CollectTiles();
        Canvas.ForceUpdateCanvases(); // GridLayoutGroup 위치 계산 후
        TileManager.AssignIndicesByPosition(tiles);
    }

    // 같은 index가 여러 타일에 있으면 그 index와 타일 이름 목록
    public List<string> FindDuplicateIndices()
    {
        var result = new List<string>();
        var map = new Dictionary<Vector2Int, List<Tile>>();
        if (tiles == null)
            return result;
        foreach (Tile tile in tiles)
        {
            if (tile == null)
                continue;
            if (!map.TryGetValue(tile.index, out List<Tile> list))
                map[tile.index] = list = new List<Tile>();
            list.Add(tile);
        }
        foreach (var pair in map)
            if (pair.Value.Count > 1)
                result.Add($"{pair.Key} : {string.Join(", ", pair.Value.ConvertAll(t => t.transform.parent != null ? $"{t.transform.parent.name}/{t.name}" : t.name))}");
        return result;
    }

    // 맵 범위 (월드): 타일 + 경로 지점. 카메라 드래그 범위에 씀 (CameraManager)
    public Bounds GetWorldBounds()
    {
        bool has = false;
        var bounds = new Bounds();
        void Add(Vector3 p)
        {
            if (!has) { bounds = new Bounds(p, Vector3.zero); has = true; }
            else bounds.Encapsulate(p);
        }
        var corners = new Vector3[4];
        foreach (Tile tile in GetComponentsInChildren<Tile>())
        {
            if (tile.transform is RectTransform rt)
            {
                rt.GetWorldCorners(corners);
                foreach (Vector3 c in corners)
                    Add(c);
            }
            else
                Add(tile.transform.position);
        }
        if (routes != null)
            foreach (StageRoute route in routes)
            {
                Transform[] points = route?.GetValidPoints();
                if (points != null)
                    foreach (Transform p in points)
                        Add(p.position);
            }
        return bounds;
    }

    // 씬 뷰: 경로(출발 초록 → 도착 빨강) + 타일 그룹 경로(enemyRoots, 하늘색)
    void OnDrawGizmos()
    {
        if (routes != null)
            foreach (StageRoute route in routes)
            {
                Transform[] points = route?.GetValidPoints();
                if (points == null || points.Length < 2)
                    continue;
                Gizmos.color = new Color(1f, 0.45f, 0.3f);
                for (int i = 0; i < points.Length - 1; i++)
                    Gizmos.DrawLine(points[i].position, points[i + 1].position);
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(points[0].position, 0.25f);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(points[points.Length - 1].position, 0.3f);
            }
        Gizmos.color = new Color(0.4f, 0.85f, 1f, 0.6f);
        foreach (TileGroup group in GetComponentsInChildren<TileGroup>())
        {
            if (group.enemyRoots == null || group.enemyRoots.Length < 2)
                continue;
            for (int i = 0; i < group.enemyRoots.Length; i++)
            {
                Transform a = group.enemyRoots[i], b = group.enemyRoots[(i + 1) % group.enemyRoots.Length];
                if (a != null && b != null)
                    Gizmos.DrawLine(a.position, b.position);
            }
        }
    }
}
