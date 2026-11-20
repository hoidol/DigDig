using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-20) 킹 버프 칸: 킹 칸 기준 offsets(종족이 정함, Tile.index 단위: x 오른쪽 +, y 아래쪽 +) 칸의 미니언에게 버프
// - TileGroup 경계 무시, 모양은 회전 없이 고정, 막힌 칸 제외
// - 버프 출처(종족·직업 등)마다 버프 목록을 등록 → 대상 미니언마다 새 Buff 인스턴스로 건다 (RemoveBuff가 참조로 지우므로)
// - 타일 오브젝트가 바뀔 때마다(킹·미니언 이동·소환·머지) 대상 재계산 (CrossBuffMinion과 같은 방식)
// - 버프 칸은 타일 위에 반투명 색(markerColor)으로 표시
public class KingTileBuffArea : MonoBehaviour
{
    KingSlime king;
    Vector2Int[] offsets = new Vector2Int[0];
    Color markerColor = new Color(1f, 0.85f, 0.3f, 0.35f);

    // 출처 → 버프 틀 (값만 쓰고 대상마다 새로 만듦)
    readonly Dictionary<object, List<Buff>> sources = new Dictionary<object, List<Buff>>();
    // 대상 → 걸어준 Buff들
    readonly Dictionary<IBuffable, List<Buff>> applied = new Dictionary<IBuffable, List<Buff>>();
    readonly List<Image> markers = new List<Image>();

    public IReadOnlyList<Vector2Int> Offsets => offsets;

    public static KingTileBuffArea GetOrCreate(KingSlime king)
    {
        var area = king.GetComponent<KingTileBuffArea>();
        if (area == null)
        {
            area = king.gameObject.AddComponent<KingTileBuffArea>();
            area.king = king;
        }
        return area;
    }

    void OnEnable() => GameEventBus.Subscribe<TileObjectChangedEvent>(OnTileObjectChanged);

    void OnDisable()
    {
        GameEventBus.Unsubscribe<TileObjectChangedEvent>(OnTileObjectChanged);
        ClearApplied();
    }

    void OnTileObjectChanged(TileObjectChangedEvent e) => Refresh();

    public void SetShape(Vector2Int[] offsets, Color markerColor)
    {
        this.offsets = offsets ?? new Vector2Int[0];
        this.markerColor = markerColor;
        ClearApplied();
        Refresh();
    }

    public void SetBuffs(object source, IEnumerable<Buff> buffs)
    {
        sources[source] = new List<Buff>(buffs);
        ClearApplied(); // 값이 바뀌었을 수 있으니 전부 다시 걸기
        Refresh();
    }

    public void RemoveBuffs(object source)
    {
        if (!sources.Remove(source))
            return;
        ClearApplied();
        Refresh();
    }

    // 지금 버프 칸 (막힌 칸·판 밖 제외). 킹이 칸 위에 없으면 빈 목록
    public List<Tile> GetTargetTiles()
    {
        var result = new List<Tile>();
        if (TileManager.Instance == null || king == null)
            return result;
        Tile center = TileManager.Instance.GetTile(king);
        if (center == null)
            return result;
        return TileManager.Instance.GetTilesAround(center, offsets);
    }

    public void Refresh()
    {
        if (!isActiveAndEnabled)
            return;
        List<Tile> tiles = GetTargetTiles();
        UpdateMarkers(tiles);

        var targets = new List<IBuffable>();
        foreach (Tile tile in tiles)
        {
            // 미니언만 (킹 자신 제외)
            if (tile.tileObject is Minion minion && minion is IBuffable target)
                targets.Add(target);
        }

        // 범위에서 빠진 대상 해제
        var removed = new List<IBuffable>();
        foreach (var pair in applied)
            if (!targets.Contains(pair.Key))
                removed.Add(pair.Key);
        foreach (IBuffable target in removed)
        {
            if (!IsDestroyed(target))
                foreach (Buff b in applied[target])
                    target.RemoveBuff(b);
            applied.Remove(target);
        }

        // 새로 들어온 대상에 버프
        foreach (IBuffable target in targets)
        {
            if (applied.ContainsKey(target))
                continue;
            var list = new List<Buff>();
            foreach (var pair in sources)
                foreach (Buff template in pair.Value)
                {
                    var buff = new Buff(template.statType, template.value, template.opType)
                    {
                        effectType = CrossBuffMinion.GetBuffEffectType(template.statType)
                    };
                    list.Add(buff);
                    target.AddBuff(buff);
                }
            applied.Add(target, list);
        }
    }

    void ClearApplied()
    {
        foreach (var pair in applied)
        {
            if (IsDestroyed(pair.Key))
                continue;
            foreach (Buff b in pair.Value)
                pair.Key.RemoveBuff(b);
        }
        applied.Clear();
    }

    static bool IsDestroyed(IBuffable target) => target == null || (target is Object obj && obj == null);

    // ---- 버프 칸 표시: 대상 타일의 자식으로 반투명 색 이미지 (터치는 막지 않음)
    void UpdateMarkers(List<Tile> tiles)
    {
        bool show = sources.Count > 0 && offsets.Length > 0;
        int count = show ? tiles.Count : 0;
        while (markers.Count < count)
            markers.Add(CreateMarker());
        for (int i = 0; i < markers.Count; i++)
        {
            Image marker = markers[i];
            if (marker == null)
                continue;
            bool active = i < count;
            marker.gameObject.SetActive(active);
            if (!active)
                continue;
            RectTransform rt = marker.rectTransform;
            rt.SetParent(tiles[i].transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
            rt.SetAsFirstSibling(); // 타일 아이템(룬) 그림보다 아래
            marker.color = markerColor;
        }
    }

    Image CreateMarker()
    {
        var go = new GameObject("KingBuffTileMarker", typeof(RectTransform), typeof(Image));
        var img = go.GetComponent<Image>();
        img.raycastTarget = false;
        return img;
    }

    void OnDestroy()
    {
        foreach (Image m in markers)
            if (m != null)
                Destroy(m.gameObject);
    }
}
