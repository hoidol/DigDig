using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MapManager : MonoSingleton<MapManager>
{
    //[SerializeField] private MapDisplay mapDisplay;
    public OreStone oreStonePrefab;

    [SerializeField] private int chunkSize = 16;
    [SerializeField] private int loadRadiusX = 3;
    [SerializeField] private int loadRadiusY = 1;
    // 이 거리에서 후순위 색상의 가중치가 크게 올라감
    //[SerializeField] private float referenceDistance = 20f;

    private Vector2Int currentPlayerChunk = new Vector2Int(int.MaxValue, int.MaxValue);
    private Dictionary<Vector2Int, List<OreStone>> loadedChunks = new Dictionary<Vector2Int, List<OreStone>>();
    private HashSet<Vector2Int> destroyedTiles = new HashSet<Vector2Int>();



    private void Awake()
    {
        //mapDisplay = GetComponent<MapDisplay>();
    }

    public Color[] fillColors;
    // 각 색상별로 거리(x축) → 가중치(y축) 커브를 Inspector에서 그래프로 설정
    public float[] fixWeights;
    public AnimationCurve[] weightCurves;
    [SerializeField] private float[] weights;
    private async void Start()
    {
        GameEventBus.Subscribe<OrdealEndEvent>(OnOrdealEndEvent);
        GameEventBus.Subscribe<ReachedOrdealEvent>(OnReachedOrdealEvent);

        weights = new float[weightCurves.Length];
        UpdateChunks();
        RunUpdateChunk().Forget();
    }
    void OnReachedOrdealEvent(ReachedOrdealEvent e)
    {
        //
    }

    void OnOrdealEndEvent(OrdealEndEvent e)
    {
        // 모든 청크 언로드 및 상태 초기화
        // foreach (var coord in new List<Vector2Int>(loadedChunks.Keys))
        //     UnloadChunk(coord);
        // destroyedTiles.Clear();
        // currentPlayerChunk = new Vector2Int(int.MaxValue, int.MaxValue);
        // UpdateChunks();
    }

    async UniTaskVoid RunUpdateChunk()
    {
        var cancellationToken = this.GetCancellationTokenOnDestroy();
        while (true)
        {
            await UniTask.Delay(1000, cancellationToken: cancellationToken);
            Vector2Int newChunk = WorldToChunkCoord(Player.Instance.transform.position);
            if (newChunk != currentPlayerChunk)
            {
                currentPlayerChunk = newChunk;
                UpdateChunks();
            }
        }

    }

    private Vector2Int WorldToChunkCoord(Vector3 worldPos)
    {
        float tileSize = chunkSize * OreStone.SIZE;
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / tileSize),
            Mathf.FloorToInt(worldPos.y / tileSize)
        );
    }

    private void UpdateChunks()
    {
        Vector2Int center = WorldToChunkCoord(Player.Instance.transform.position);
        currentPlayerChunk = center;

        // 범위 밖 청크 언로드
        List<Vector2Int> toUnload = new List<Vector2Int>();
        foreach (var coord in loadedChunks.Keys)
        {
            if (Mathf.Abs(coord.x - center.x) > loadRadiusX ||
                Mathf.Abs(coord.y - center.y) > loadRadiusY)
            {
                toUnload.Add(coord);
            }
        }
        foreach (var coord in toUnload)
            UnloadChunk(coord);

        // 범위 안 청크 로드
        for (int dy = -loadRadiusY; dy <= loadRadiusY; dy++)
        {
            for (int dx = -loadRadiusX; dx <= loadRadiusX; dx++)
            {
                Vector2Int chunkCoord = new Vector2Int(center.x + dx, center.y + dy);
                if (!loadedChunks.ContainsKey(chunkCoord))
                    LoadChunk(chunkCoord);
            }
        }
    }

    private void LoadChunk(Vector2Int chunkCoord)
    {
        List<OreStone> tiles = new List<OreStone>();
        int startX = chunkCoord.x * chunkSize;
        int startY = chunkCoord.y * chunkSize;

        for (int dy = 0; dy < chunkSize; dy++)
        {
            for (int dx = 0; dx < chunkSize; dx++)
            {
                Vector2Int gridPos = new Vector2Int(startX + dx, startY + dy);
                Vector3 worldPos = new Vector3(gridPos.x * OreStone.SIZE, gridPos.y * OreStone.SIZE, 0f);

                // Vector2.zero 기준 5 이하 거리는 스폰 금지
                if (Vector2.Distance(Vector2.zero, worldPos) <= 5f) continue;

                // 파괴된 타일은 스폰 금지
                if (destroyedTiles.Contains(gridPos)) continue;

                float distance = Vector2.Distance(Vector2.zero, worldPos);
                int colorIdx = PickColorIndex(distance);
                Color color = fillColors[colorIdx];

                OreStone oreStone = OreStone.Get(oreStonePrefab, worldPos, transform);
                oreStone.Init(colorIdx, color, gridPos);
                tiles.Add(oreStone);
            }
        }

        loadedChunks[chunkCoord] = tiles;
    }

    private void UnloadChunk(Vector2Int chunkCoord)
    {
        if (!loadedChunks.TryGetValue(chunkCoord, out List<OreStone> tiles)) return;
        foreach (var tile in tiles)
        {
            if (tile != null)
                tile.Return();
        }
        loadedChunks.Remove(chunkCoord);
    }

    // fixWeights[i]: 거리 무관 고정 가중치
    // weightCurves[i]: x=0~1 (distance/100), y=추가 가중치
    private int PickColorIndex(float distance)
    {
        float rate = Mathf.Clamp01(distance / 100f);
        int n = weightCurves.Length;
        float total = 0f;
        //Debug.Log($"distance : {distance}, rate {rate}");
        for (int i = 0; i < n; i++)
        {
            //Debug.Log($"i : {i}, weightCurves[i].Evaluate(rate) {weightCurves[i].Evaluate(rate)}");
            weights[i] = Mathf.Max(0f, weightCurves[i].Evaluate(rate));
            total += weights[i];
        }

        float rand = Random.Range(0f, total);
        float cumulative = 0f;
        for (int i = 0; i < n; i++)
        {
            cumulative += weights[i];
            if (rand < cumulative) return i;
        }
        return n - 1;
    }

    public void RegisterDestroyed(Vector2Int gridPos)
    {
        if (!destroyedTiles.Contains(gridPos))
            destroyedTiles.Add(gridPos);
    }

    // NPC 등장 시 호출 - 반경 내 타일 제거 및 재생성 방지
    public void ClearTilesInRadius(Vector2 center, float radius)
    {
        int gridRadius = Mathf.CeilToInt(radius / OreStone.SIZE);
        Vector2Int centerGrid = new Vector2Int(
            Mathf.RoundToInt(center.x / OreStone.SIZE),
            Mathf.RoundToInt(center.y / OreStone.SIZE)
        );
        for (int dy = -gridRadius; dy <= gridRadius; dy++)
        {
            for (int dx = -gridRadius; dx <= gridRadius; dx++)
            {
                Vector2Int gridPos = new Vector2Int(centerGrid.x + dx, centerGrid.y + dy);
                Vector3 worldPos = new Vector3(gridPos.x * OreStone.SIZE, gridPos.y * OreStone.SIZE);
                if (Vector2.Distance(center, worldPos) <= radius)
                    destroyedTiles.Add(gridPos);
            }
        }
        // 현재 로드된 타일 즉시 제거
        Collider2D[] cols = Physics2D.OverlapCircleAll(center, radius, LayerMask.GetMask("Hittable"));
        foreach (var col in cols)
        {
            if (col.tag == "OreStone")
            {
                col.GetComponent<OreStone>()?.Destroyed(false);
            }

        }

    }
}
