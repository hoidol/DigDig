using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-20) 킹 버프 칸: 킹 칸 기준 offsets(Tile.index 단위: x 오른쪽 +, y 아래쪽 +) 칸의 미니언에게 버프
// - (2026-11-21) 칸 모양도 출처별로 등록 (종족 3칸 + 직업 1칸 …) → 버프 칸 = 합친 모양. 모든 출처의 버프가 모든 버프 칸에 걸림
// - TileGroup 경계 무시, 모양은 회전 없이 고정, 막힌 칸 제외
// - 버프 출처(종족·직업 등)마다 버프 목록을 등록 → 대상 미니언마다 새 Buff 인스턴스로 건다 (RemoveBuff가 참조로 지우므로)
// - 타일 오브젝트가 바뀔 때마다(킹·미니언 이동·소환·머지) 대상 재계산 (CrossBuffMinion과 같은 방식)
// - 버프 칸은 타일 위에 반투명 색(markerColor)으로 표시
// - (2026-11-21) 버프 배율(IKingBuffAreaScaler, 오크 부족의 결속 등): 대상 미니언 구성에 따라 모든 칸 버프를 ×N
//   TilesRefreshed: 버프 칸·대상이 다시 계산될 때마다 알림 (칸 위 룬·미니언에 직접 효과를 주는 패시브용)
public interface IKingBuffAreaScaler
{
    // 1 = 그대로. targets = 지금 버프 칸 위 미니언
    float GetBuffScale(IReadOnlyList<Minion> targets);
}

public class KingTileBuffArea : MonoBehaviour
{
    public event System.Action<KingTileBuffArea, List<Tile>> TilesRefreshed;
    readonly List<IKingBuffAreaScaler> scalers = new List<IKingBuffAreaScaler>();
    float appliedScale = 1f;
    public float BuffScale => appliedScale;
    KingSlime king;
    Vector2Int[] offsets = new Vector2Int[0]; // 출처별 모양을 합친 것
    readonly Dictionary<object, Vector2Int[]> shapes = new Dictionary<object, Vector2Int[]>();
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

    // source 출처의 칸 모양 등록. markerColor를 주면 표시 색도 바꿈 (종족 색)
    public void SetShape(object source, Vector2Int[] offsets, Color? markerColor = null)
    {
        shapes[source] = offsets ?? new Vector2Int[0];
        if (markerColor.HasValue)
            this.markerColor = markerColor.Value;
        RebuildOffsets();
    }

    public void RemoveShape(object source)
    {
        if (shapes.Remove(source))
            RebuildOffsets();
    }

    void RebuildOffsets()
    {
        var list = new List<Vector2Int>();
        foreach (var shape in shapes.Values)
            foreach (var o in shape)
                if (!list.Contains(o))
                    list.Add(o);
        offsets = list.ToArray();
        ClearApplied();
        Refresh();
    }

    public void SetBuffs(object source, IEnumerable<Buff> buffs)
    {
        sources[source] = new List<Buff>(buffs);
        ClearApplied(); // 값이 바뀌었을 수 있으니 전부 다시 걸기
        Refresh();
    }

    // ---- (2026-11-21) 버프 칸 정보 (빈 버프 칸·버프 칸 미니언을 누르면 룬 정보 자리에 표시, TileItemInfoPanel)
    public static KingTileBuffArea Current => KingSlime.Instance != null ? KingSlime.Instance.GetComponent<KingTileBuffArea>() : null;

    // 지금 킹 버프 칸인지 (버프가 하나라도 걸려 있을 때만)
    public static bool IsBuffTile(Tile tile)
    {
        KingTileBuffArea area = Current;
        return tile != null && area != null && area.isActiveAndEnabled && area.sources.Count > 0 && area.GetTargetTiles().Contains(tile);
    }

    // 출처(종족·직업·패시브)마다 한 줄: "오크 : 공격력 +10%". 칸 버프 배율이 걸려 있으면 실제 값 + 배율 줄
    public string GetBuffDescription()
    {
        var lines = new List<string>();
        foreach (var pair in sources)
        {
            if (pair.Value.Count == 0)
                continue;
            string name = pair.Key is KingAbility ability && ability.data != null ? ability.data.title : "";
            var parts = new List<string>();
            foreach (Buff b in pair.Value)
                parts.Add(FormatBuff(b.statType, ScaleBuffValue(b, appliedScale), b.opType));
            lines.Add($"<b>{name}</b> : {string.Join(", ", parts)}");
        }
        if (!Mathf.Approximately(appliedScale, 1f))
        {
            var names = new List<string>();
            foreach (var scaler in scalers)
                if (scaler is KingAbility ability && ability.data != null && !Mathf.Approximately(scaler.GetBuffScale(GetTargetMinions()), 1f))
                    names.Add(ability.data.title);
            lines.Add($"<color=#FFB13B>{string.Join(", ", names)} : 칸 버프 ×{appliedScale:0.##} 적용 중</color>");
        }
        return string.Join("\n", lines);
    }

    // 버프 하나 글씨: Multiply = 증가분 %, Add = 사거리만 값 그대로, 나머지는 %
    public static string FormatBuff(StatType statType, float value, StatOpType opType)
    {
        string title = TranslateManager.GetText($"{statType}_title");
        if (opType == StatOpType.Multiply)
        {
            float pct = (value - 1f) * 100f;
            return $"{title} {(pct >= 0 ? "+" : "")}{pct:0.##}%";
        }
        string sign = value >= 0 ? "+" : "";
        return statType == StatType.AttackRange ? $"{title} {sign}{value:0.##}" : $"{title} {sign}{value:0.##}%";
    }

    public void AddScaler(IKingBuffAreaScaler scaler)
    {
        if (!scalers.Contains(scaler))
            scalers.Add(scaler);
        Refresh();
    }

    public void RemoveScaler(IKingBuffAreaScaler scaler)
    {
        if (scalers.Remove(scaler))
            Refresh();
    }

    // 지금 버프 칸 위 미니언
    public List<Minion> GetTargetMinions()
    {
        var result = new List<Minion>();
        foreach (Tile tile in GetTargetTiles())
            if (tile.tileObject is Minion minion && minion != null)
                result.Add(minion);
        return result;
    }

    // 버프 값 × 배율: Multiply = 증가분만 키움(1.1 ×2 → 1.2), Add = 값 그대로 곱함
    public static float ScaleBuffValue(Buff template, float scale)
    {
        if (Mathf.Approximately(scale, 1f))
            return template.value;
        return template.opType == StatOpType.Multiply ? 1f + (template.value - 1f) * scale : template.value * scale;
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
        var minions = new List<Minion>();
        foreach (Tile tile in tiles)
        {
            // 미니언만 (킹 자신 제외)
            if (tile.tileObject is Minion minion && minion is IBuffable target)
            {
                targets.Add(target);
                minions.Add(minion);
            }
        }

        // 배율이 바뀌면 전부 다시 걸기
        float scale = 1f;
        foreach (var scaler in scalers)
            scale *= scaler.GetBuffScale(minions);
        if (!Mathf.Approximately(scale, appliedScale))
        {
            ClearApplied();
            appliedScale = scale;
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
                    var buff = new Buff(template.statType, ScaleBuffValue(template, appliedScale), template.opType)
                    {
                        effectType = CrossBuffMinion.GetBuffEffectType(template.statType)
                    };
                    list.Add(buff);
                    target.AddBuff(buff);
                }
            applied.Add(target, list);
        }

        TilesRefreshed?.Invoke(this, tiles);
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
