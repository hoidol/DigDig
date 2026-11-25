using System.Collections.Generic;
using UnityEngine;

// (2026-11-24) 브레이크타임 적 경로 표시: 흐리게 깔린 경로선 + 도착 위치 쪽으로 흘러가는 화살표
// 경로 = EnemyFieldController.GetRoutes() (출발 위치 여럿 가능, 도착 위치는 하나). 갈래가 합쳐지는 곳부터는 한 번만 그림
// 화살표 위치는 "도착까지 남은 거리" 기준 → 갈래가 합쳐져도 화살표 간격·흐름이 끊기지 않음
// InGame 씬 EnemyFieldController 오브젝트에 붙어 있음 (없으면 실행 중 기본값으로 붙음)
// 디자인: 인스펙터 값은 실행 중에도 바로 반영. 편집 모드에서도 씬에 미리보기(previewInEditor), 선택하면 화살표가 흐름
public enum PathArrowMode
{
    Runner, // 화살표 몇 개가 한 묶음으로 출발 → 도착까지 빠르게 달려감 (반복)
    Flow, // 경로 전체에 화살표가 깔려 천천히 흐름
}

public enum PathArrowColorMode
{
    Single, // arrowColor 하나
    Gradient, // 출발(왼쪽) → 도착(오른쪽) 색 arrowGradient
}

[ExecuteAlways]
public class EnemyPathPreview : MonoBehaviour
{
    [Header("경로선")]
    public bool showLine = true;
    public float lineWidth = 0.22f;
    public Color lineColor = new Color(1f, 0.42f, 0.35f, 0.22f);
    public Material lineMaterial; // 비우면 Sprites/Default. 점선 등 그림을 쓰려면 텍스처 있는 머티리얼 + lineTextureMode Tile
    public LineTextureMode lineTextureMode = LineTextureMode.Stretch;
    public float lineTextureScrollSpeed; // 0이 아니면 선 그림이 흐름 (실행 중만, 머티리얼 _MainTex 오프셋)
    [Range(0, 10)] public int lineCornerVertices = 4; // 꺾이는 곳 둥글기
    [Range(0, 10)] public int lineCapVertices = 4; // 양 끝 둥글기

    [Header("화살표")]
    public bool showArrows = true;
    public PathArrowMode arrowMode = PathArrowMode.Runner;
    public Sprite arrowSprite; // 비우면 코드로 만든 꺾쇠(>)
    public float arrowSpriteAngle; // 그림이 오른쪽(+x)이 아닌 방향을 보면 보정 (예: 위쪽 그림 = -90)
    public Material arrowMaterial; // 비우면 기본 스프라이트 머티리얼
    public float arrowSize = 0.32f; // 화살표 크기 (월드 단위, 그림의 긴 쪽 기준)
    public PathArrowColorMode arrowColorMode = PathArrowColorMode.Single;
    public Color arrowColor = new Color(1f, 0.42f, 0.35f, 0.9f);
    public Gradient arrowGradient = DefaultGradient(); // 출발 → 도착
    [Header("화살표 - Runner (달려가는 묶음)")]
    public float runnerSpeed = 7f; // 초당 이동 거리 (실제 시간 기준, 배속 무관)
    [Range(1, 10)] public int runnerCount = 3; // 한 묶음 화살표 수 (뒤로 갈수록 흐려짐)
    [Range(1, 5)] public int runnerGroups = 3; // 한 경로에 동시에 달리는 묶음 수 (같은 간격으로 뒤따름)
    public float runnerGap = 0.3f; // 묶음 안 화살표 간격
    public float runnerInterval = 0.8f; // 도착 후 다음 묶음이 출발하기까지 쉬는 시간
    [Range(0f, 1f)] public float runnerTailAlpha = 0.3f; // 묶음 맨 뒤 화살표 진하기 (맨 앞 = 1)

    [Header("화살표 - Flow (경로 전체)")]
    public float arrowSpacing = 0.7f; // 화살표 간격
    public float arrowSpeed = 1.1f; // 초당 이동 거리 (실제 시간 기준, 배속 무관)

    [Header("화살표 - 공통")]
    public float edgeFadeDistance = 0.5f; // 출발·도착 근처에서 화살표가 흐려지는 거리
    [Range(0f, 1f)] public float arrowPulse; // 화살표가 커졌다 작아지는 정도 (0 = 없음)
    public float arrowPulseSpeed = 4f;

    [Header("표시")]
    public string sortingLayerName = "UnderDefault";
    public int sortingOrder = 10; // 선 = 이 값, 화살표 = +1
    public float fadeTime = 0.3f; // 브레이크타임 시작·wave 시작 때 나타나고 사라지는 시간
    public float dragAlphaMul = 1.6f; // 카드·유닛 드래그 중엔 더 진하게 (배치 판단용)
    public float mergeStartDistance = 1f; // 출발 위치끼리 이 거리 안이고 다음 지점부터 같은 길이면 한 줄로 표시

    [Header("디자인 확인")]
    public bool previewInEditor = true; // 편집 모드에서 씬에 미리보기 (저장 안 됨)
    public bool alwaysShowInPlay; // 실행 중 브레이크타임이 아니어도 항상 보이기

    // 그릴 조각: 출발 위치부터 (다른 갈래와 합쳐지는 지점 또는 도착 위치)까지
    class Piece
    {
        public Vector2[] points;
        public float[] distToEnd; // 각 지점에서 도착 위치까지 남은 거리 (내림차순)
    }

    readonly List<Piece> pieces = new(); // 선 (합쳐진 구간은 한 번만)
    readonly List<Piece> routes = new(); // 출발 → 도착 전체 경로 (Runner용)
    float runnerTime;
    readonly List<LineRenderer> lines = new();
    readonly List<SpriteRenderer> arrows = new();
    Transform root;
    float alpha; // 전체 표시 정도 0~1
    float flow; // 흐른 거리
    float pulseTime;
    float maxDistToEnd = 1f; // 그라데이션 기준 (가장 먼 출발 위치)
    bool wasVisible;
    bool wasVisibleForRunner;
    double lastEditorTime;
    static Sprite defaultArrowSprite;
    static Material defaultLineMaterial;

    static Gradient DefaultGradient()
    {
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.75f, 0.35f), 0f), new GradientColorKey(new Color(1f, 0.35f, 0.3f), 1f) },
            new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0.9f, 1f) });
        return g;
    }

    bool Editing => !Application.isPlaying;
    HideFlags CreateFlags => Editing ? HideFlags.HideAndDontSave : HideFlags.None;

    void OnDisable() => ClearAll();

    void OnDestroy() => ClearAll();

    void ClearAll()
    {
        if (root != null)
        {
            if (Application.isPlaying)
                Destroy(root.gameObject);
            else
                DestroyImmediate(root.gameObject);
        }
        root = null;
        lines.Clear();
        arrows.Clear();
        pieces.Clear();
        wasVisible = false;
        alpha = 0f;
    }

    void EnsureRoot()
    {
        if (root != null)
            return;
        var go = new GameObject("EnemyPathPreview") { hideFlags = CreateFlags };
        root = go.transform;
        root.SetParent(transform, false);
    }

    // 경로가 바뀌면 다시 호출 (실행 중엔 보일 때마다, 편집 모드에선 매번 다시 계산)
    public void Rebuild()
    {
        EnsureRoot();
        pieces.Clear();
        routes.Clear();
        EnemyFieldController field = GetComponent<EnemyFieldController>();
        if (field == null)
            field = EnemyFieldController.Instance;
        if (field != null)
        {
            List<List<Vector2>> merged = MergeNearStarts(field.GetRoutes(), mergeStartDistance);
            pieces.AddRange(BuildPieces(merged));
            foreach (List<Vector2> route in merged)
                routes.AddRange(BuildPieces(new List<List<Vector2>> { route })); // 갈래마다 끝까지
        }

        maxDistToEnd = 0.01f;
        foreach (Piece piece in pieces)
            maxDistToEnd = Mathf.Max(maxDistToEnd, piece.distToEnd[0]);

        while (lines.Count < pieces.Count)
            lines.Add(CreateLine());
        for (int i = 0; i < lines.Count; i++)
        {
            bool use = i < pieces.Count;
            if (lines[i].gameObject.activeSelf != use)
                lines[i].gameObject.SetActive(use);
            if (!use)
                continue;
            Vector2[] points = pieces[i].points;
            lines[i].positionCount = points.Length;
            for (int p = 0; p < points.Length; p++)
                lines[i].SetPosition(p, points[p]);
        }
    }

    // 출발 위치가 가깝고(mergeStartDistance 이내) 바로 다음 지점부터 같은 길이면 경로 하나로 합쳐 그림 (출발 = 가운데)
    // 예: 스테이지 1 = 소환 지점 2개가 붙어 있고 바로 paths[1]에서 합쳐짐 → V자 두 줄 대신 한 줄
    public static List<List<Vector2>> MergeNearStarts(List<List<Vector2>> routes, float mergeDistance)
    {
        var result = new List<List<Vector2>>();
        var counts = new List<int>();
        foreach (List<Vector2> route in routes)
        {
            if (route == null || route.Count < 2)
                continue;
            int found = -1;
            for (int i = 0; i < result.Count && found < 0; i++)
                if (SameTail(result[i], route) && Vector2.Distance(result[i][0], route[0]) <= mergeDistance)
                    found = i;
            if (found < 0)
            {
                result.Add(new List<Vector2>(route));
                counts.Add(1);
                continue;
            }
            // 합친 출발 위치 = 출발 위치들의 평균
            result[found][0] = (result[found][0] * counts[found] + route[0]) / (counts[found] + 1);
            counts[found]++;
        }
        return result;
    }

    static bool SameTail(List<Vector2> a, List<Vector2> b)
    {
        if (a.Count != b.Count)
            return false;
        for (int i = 1; i < a.Count; i++)
            if (Key(a[i]) != Key(b[i]))
                return false;
        return true;
    }

    // 갈래마다 이미 그린 지점을 만나기 전까지만 남김 (합쳐진 뒤 구간은 먼저 그린 갈래가 맡음)
    static List<Piece> BuildPieces(List<List<Vector2>> routes)
    {
        var result = new List<Piece>();
        var covered = new HashSet<Vector2Int>();
        foreach (List<Vector2> route in routes)
        {
            if (route == null || route.Count < 2)
                continue;

            var dist = new float[route.Count];
            for (int i = route.Count - 2; i >= 0; i--)
                dist[i] = dist[i + 1] + Vector2.Distance(route[i], route[i + 1]);

            int last = route.Count - 1;
            for (int i = 1; i < route.Count; i++)
                if (covered.Contains(Key(route[i])))
                {
                    last = i;
                    break;
                }

            foreach (Vector2 p in route)
                covered.Add(Key(p));

            int count = last + 1;
            var piece = new Piece { points = new Vector2[count], distToEnd = new float[count] };
            for (int i = 0; i < count; i++)
            {
                piece.points[i] = route[i];
                piece.distToEnd[i] = dist[i];
            }
            result.Add(piece);
        }
        return result;
    }

    static Vector2Int Key(Vector2 p) => new(Mathf.RoundToInt(p.x * 100f), Mathf.RoundToInt(p.y * 100f));

    LineRenderer CreateLine()
    {
        var go = new GameObject("PathLine") { hideFlags = CreateFlags };
        go.transform.SetParent(root, false);
        var line = go.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.alignment = LineAlignment.TransformZ;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        return line;
    }

    static Material DefaultLineMaterial
    {
        get
        {
            if (defaultLineMaterial == null)
                defaultLineMaterial = new Material(Shader.Find("Sprites/Default")) { name = "EnemyPathLine", hideFlags = HideFlags.HideAndDontSave };
            return defaultLineMaterial;
        }
    }

    void Update()
    {
        bool visible;
        float dt;
        if (Editing)
        {
            if (!previewInEditor)
            {
                if (root != null)
                    ClearAll();
                return;
            }
            visible = true;
            double now = Time.realtimeSinceStartupAsDouble;
            dt = lastEditorTime > 0 ? Mathf.Min((float)(now - lastEditorTime), 0.1f) : 0f;
            lastEditorTime = now;
            Rebuild(); // 편집 중엔 경로 지점을 옮기면 바로 따라감
            alpha = 1f;
        }
        else
        {
            GameManager game = GameManager.Instance;
            visible = alwaysShowInPlay || (game != null && game.isPlaying && game.isBreakTime);
            if (visible && !wasVisible)
            {
                Rebuild();
                root.gameObject.SetActive(true);
            }
            dt = Time.unscaledDeltaTime;
            alpha = Mathf.MoveTowards(alpha, visible ? 1f : 0f, fadeTime > 0f ? dt / fadeTime : 1f);
        }
        wasVisible = visible;

        if (root == null)
            return;
        if (alpha <= 0f)
        {
            if (root.gameObject.activeSelf)
                root.gameObject.SetActive(false);
            return;
        }
        if (!root.gameObject.activeSelf)
            root.gameObject.SetActive(true);

        flow += arrowSpeed * dt;
        if (arrowSpacing > 0f)
            flow = Mathf.Repeat(flow, arrowSpacing);
        pulseTime += dt;
        runnerTime += dt;
        if (visible && !wasVisibleForRunner)
            runnerTime = 0f; // 보이기 시작하면 첫 묶음이 바로 출발
        wasVisibleForRunner = visible;
        float a = alpha * (!Editing && IsDragging() ? dragAlphaMul : 1f);

        UpdateLines(a, dt);
        UpdateArrows(a);

#if UNITY_EDITOR
        // 편집 모드: 이 오브젝트를 선택하고 있는 동안만 계속 그려서 화살표가 흐르게
        if (Editing && UnityEditor.Selection.activeGameObject == gameObject)
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
        }
#endif
    }

    void UpdateLines(float a, float dt)
    {
        Color lc = lineColor;
        lc.a = Mathf.Clamp01(lc.a * a);
        Material mat = lineMaterial != null ? lineMaterial : DefaultLineMaterial;
        for (int i = 0; i < pieces.Count && i < lines.Count; i++)
        {
            LineRenderer line = lines[i];
            line.enabled = showLine;
            if (!showLine)
                continue;
            line.startColor = line.endColor = lc;
            line.widthMultiplier = lineWidth;
            line.numCornerVertices = lineCornerVertices;
            line.numCapVertices = lineCapVertices;
            line.textureMode = lineTextureMode;
            line.sortingLayerName = sortingLayerName;
            line.sortingOrder = sortingOrder;
            // 흐르는 선 그림 = 선마다 머티리얼 복사본이 필요 → 실행 중에만
            if (!Editing && lineTextureScrollSpeed != 0f && lineMaterial != null)
            {
                if (line.sharedMaterial == null || line.sharedMaterial.name != lineMaterial.name + " (PathScroll)")
                    line.sharedMaterial = new Material(lineMaterial) { name = lineMaterial.name + " (PathScroll)" };
                Material m = line.sharedMaterial;
                m.mainTextureOffset = new Vector2(m.mainTextureOffset.x - lineTextureScrollSpeed * dt, m.mainTextureOffset.y);
            }
            else if (line.sharedMaterial != mat)
                line.sharedMaterial = mat;
        }
    }

    static bool IsDragging()
    {
        if (MinionFieldController.Instance != null && MinionFieldController.Instance.IsDragging)
            return true;
        return MinionOfferSystem.Instance != null && MinionOfferSystem.Instance.IsCardDragging;
    }

    void UpdateArrows(float a)
    {
        int used = 0;
        Sprite sprite = arrowSprite != null ? arrowSprite : DefaultArrowSprite;
        float spriteSize = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
        float baseScale = spriteSize > 0f ? arrowSize / spriteSize : 1f;
        if (showArrows && arrowMode == PathArrowMode.Runner && runnerSpeed > 0.01f)
        {
            // 갈래마다 묶음 하나가 출발 → 도착까지 달리고 runnerInterval 쉬었다 다시. 갈래끼리는 출발 시점을 조금씩 엇갈림
            for (int r = 0; r < routes.Count; r++)
            {
                Piece route = routes[r];
                float length = route.distToEnd[0];
                float tail = runnerGap * (runnerCount - 1);
                float cycle = (length + tail) / runnerSpeed + Mathf.Max(0f, runnerInterval);
                float routeOffset = r * cycle / Mathf.Max(1, routes.Count) * 0.5f;
                int groupCount = Mathf.Max(1, runnerGroups);
                for (int g = 0; g < groupCount; g++)
                {
                    // 묶음마다 출발 시각을 늦춤 → 보이기 시작할 때도 모든 묶음이 출발 위치에서 차례로 나옴 (중간에서 갑자기 나타나지 않음)
                    float t = runnerTime - routeOffset - g * cycle / groupCount;
                    if (t < 0f)
                        continue;
                    float phase = Mathf.Repeat(t, cycle);
                    float head = phase * runnerSpeed; // 출발에서 맨 앞 화살표까지 간 거리
                    for (int k = 0; k < runnerCount; k++)
                    {
                        float traveled = head - k * runnerGap;
                        if (traveled < 0f || traveled > length)
                            continue;
                        float d = length - traveled; // 도착까지 남은 거리
                        float trail = runnerCount > 1 ? Mathf.Lerp(1f, runnerTailAlpha, k / (float)(runnerCount - 1)) : 1f;
                        PlaceArrow(used++, route, d, a * trail, sprite, baseScale);
                    }
                }
            }
        }
        else if (showArrows && arrowMode == PathArrowMode.Flow && arrowSpacing > 0.01f)
        {
            foreach (Piece piece in pieces)
            {
                float start = piece.distToEnd[0];
                float end = piece.distToEnd[piece.distToEnd.Length - 1];
                // 도착까지 남은 거리 d = n × 간격 - 흐른 거리 → 시간이 지나면 d가 줄어 도착 쪽으로 이동
                int n = Mathf.FloorToInt((end + flow) / arrowSpacing) + 1;
                for (float d = n * arrowSpacing - flow; d <= start; d += arrowSpacing)
                {
                    if (d <= end)
                        continue;
                    PlaceArrow(used++, piece, d, a, sprite, baseScale);
                }
            }
        }
        for (int i = used; i < arrows.Count; i++)
            if (arrows[i].gameObject.activeSelf)
                arrows[i].gameObject.SetActive(false);
    }

    // 경로(piece) 위 도착까지 남은 거리 d 지점에 화살표 하나
    void PlaceArrow(int idx, Piece piece, float d, float a, Sprite sprite, float baseScale)
    {
        float start = piece.distToEnd[0];
        GetPoint(piece, d, out Vector2 pos, out Vector2 dir);
        float edge = edgeFadeDistance > 0f ? Mathf.Clamp01(Mathf.Min(start - d, d) / edgeFadeDistance) : 1f;
        SpriteRenderer arrow = GetArrow(idx);
        if (arrow.sprite != sprite)
            arrow.sprite = sprite;
        if (arrowMaterial != null && arrow.sharedMaterial != arrowMaterial)
            arrow.sharedMaterial = arrowMaterial;
        arrow.sortingLayerName = sortingLayerName;
        arrow.sortingOrder = sortingOrder + 1;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + arrowSpriteAngle;
        arrow.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, 0f, angle));
        float pulse = arrowPulse > 0f ? 1f + arrowPulse * 0.5f * Mathf.Sin(pulseTime * arrowPulseSpeed - d * 3f) : 1f;
        arrow.transform.localScale = Vector3.one * (baseScale * pulse);
        Color c = arrowColorMode == PathArrowColorMode.Gradient && arrowGradient != null
            ? arrowGradient.Evaluate(1f - d / maxDistToEnd)
            : arrowColor;
        c.a = Mathf.Clamp01(c.a * a * edge);
        arrow.color = c;
    }

    static void GetPoint(Piece piece, float d, out Vector2 pos, out Vector2 dir)
    {
        for (int i = 0; i < piece.points.Length - 1; i++)
        {
            float d0 = piece.distToEnd[i];
            float d1 = piece.distToEnd[i + 1];
            if (d > d0 || d < d1)
                continue;
            float t = d0 - d1 > 0.0001f ? (d0 - d) / (d0 - d1) : 0f;
            pos = Vector2.Lerp(piece.points[i], piece.points[i + 1], t);
            dir = (piece.points[i + 1] - piece.points[i]).normalized;
            return;
        }
        pos = piece.points[piece.points.Length - 1];
        dir = Vector2.right;
    }

    SpriteRenderer GetArrow(int idx)
    {
        while (arrows.Count <= idx)
        {
            var go = new GameObject("PathArrow") { hideFlags = CreateFlags };
            go.transform.SetParent(root, false);
            arrows.Add(go.AddComponent<SpriteRenderer>());
        }
        SpriteRenderer arrow = arrows[idx];
        if (!arrow.gameObject.activeSelf)
            arrow.gameObject.SetActive(true);
        return arrow;
    }

    // 기본 화살표: 오른쪽을 향한 흰 꺾쇠(>) — 색은 arrowColor로 입힘
    static Sprite DefaultArrowSprite
    {
        get
        {
            if (defaultArrowSprite != null)
                return defaultArrowSprite;
            const int size = 64;
            const float half = 5.5f; // 선 두께 절반 (픽셀)
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "EnemyPathArrow",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            Vector2 top = new(18f, 54f), tip = new(46f, 32f), bottom = new(18f, 10f);
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new(x + 0.5f, y + 0.5f);
                    float dist = Mathf.Min(DistToSegment(p, top, tip), DistToSegment(p, bottom, tip));
                    byte alpha = (byte)(Mathf.Clamp01(half + 0.5f - dist) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            tex.SetPixels32(pixels);
            tex.Apply();
            defaultArrowSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            defaultArrowSprite.hideFlags = HideFlags.HideAndDontSave;
            return defaultArrowSprite;
        }
    }

    static float DistToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
        return Vector2.Distance(p, a + ab * t);
    }
}
