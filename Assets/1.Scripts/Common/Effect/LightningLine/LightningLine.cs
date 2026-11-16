using System.Collections.Generic;
using UnityEngine;

// LineRenderer로 전기가 흐르는 선 (CartoonVFX9X Beam_Yellow_1 리소스를 선으로 옮긴 것)
// - coreLine: 번개 시트(Lightning_Beam_Sheet, 세로 5줄)를 frameInterval마다 랜덤 줄로 바꿔서 지지직
// - glowLine: 레이저 시트(Beam_Laser_Sheet_1_Yellow, 세로 2줄)를 번갈아 → 바탕 빛
// - startFx / endFx: 양 끝에 둘 오브젝트(플레어·스파크 파티클), 선의 첫 점 / 마지막 점으로 따라감
// 사용: Play(from, to, duration) 또는 SetPoints(points) + Play(duration). duration <= 0이면 Stop()까지 유지.
public class LightningLine : MonoBehaviour
{
    static readonly int MAIN_TEX_ST = Shader.PropertyToID("_MainTex_ST");

    [System.Serializable]
    public class SheetLine
    {
        public LineRenderer line;
        [Tooltip("스프라이트 시트 세로 칸 수")] public int rows = 1;
        [Tooltip("프레임마다 랜덤 줄(false = 순서대로)")] public bool randomRow = true;
        [Tooltip("프레임마다 위아래 뒤집기 랜덤(모양 다양하게)")] public bool randomFlip = true;
        [Tooltip("기본 두께")] public float width = 1f;
        [Tooltip("프레임마다 두께 흔들림 비율(0.2 = ±20%)")] public float widthJitter = 0.2f;

        [System.NonSerialized] public MaterialPropertyBlock block;
        [System.NonSerialized] public int row;
    }

    public SheetLine coreLine = new SheetLine { rows = 5, width = 0.6f };
    public SheetLine glowLine = new SheetLine { rows = 2, randomRow = false, randomFlip = false, width = 0.4f, widthJitter = 0.1f };

    [Tooltip("시트 프레임 교체 간격(초). 원본 파티클 = 0.3초에 5칸 × 2회 = 0.03초")]
    public float frameInterval = 0.03f;
    // 두께 배율(코드에서 조절, 예: 광선 미니언 머지 레벨). coreLine·glowLine 두께에 곱함
    [System.NonSerialized] public float widthScale = 1f;
    [Tooltip("true면 Time.unscaledDeltaTime 사용")]
    public bool useUnscaledTime;

    public Transform startFx;
    public Transform endFx;

    readonly List<Vector3> points = new List<Vector3>();
    float frameTimer;
    float remainTime;
    bool isPlaying;

    public bool IsPlaying => isPlaying;

    void Awake()
    {
        InitLine(coreLine);
        InitLine(glowLine);
        if (!isPlaying)
            SetVisible(false);
    }

    void InitLine(SheetLine sheet)
    {
        if (sheet.line == null)
            return;
        sheet.block = new MaterialPropertyBlock();
        sheet.line.useWorldSpace = true;
        sheet.line.textureMode = LineTextureMode.Stretch;
    }

    public void Play(Vector3 from, Vector3 to, float duration = 0f)
    {
        points.Clear();
        points.Add(from);
        points.Add(to);
        ApplyPoints();
        Play(duration);
    }

    public void SetPoints(IList<Vector3> newPoints)
    {
        points.Clear();
        points.AddRange(newPoints);
        ApplyPoints();
    }

    // 끝점만 옮김(대상을 따라가는 선)
    public void SetEndPoints(Vector3 from, Vector3 to)
    {
        if (points.Count < 2)
        {
            Play(from, to, remainTime);
            return;
        }
        points[0] = from;
        points[points.Count - 1] = to;
        ApplyPoints();
    }

    public void Play(float duration = 0f)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        isPlaying = true;
        remainTime = duration;
        frameTimer = 0f;
        SetVisible(true);
        NextFrame();
    }

    public void Stop()
    {
        isPlaying = false;
        SetVisible(false);
    }

    void Update()
    {
        if (!isPlaying)
            return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (remainTime > 0f)
        {
            remainTime -= dt;
            if (remainTime <= 0f)
            {
                Stop();
                return;
            }
        }

        frameTimer += dt;
        if (frameTimer >= frameInterval)
        {
            frameTimer = 0f;
            NextFrame();
        }
    }

    void ApplyPoints()
    {
        ApplyPoints(coreLine);
        ApplyPoints(glowLine);

        if (points.Count == 0)
            return;
        if (startFx != null)
            startFx.position = points[0];
        if (endFx != null)
            endFx.position = points[points.Count - 1];
    }

    void ApplyPoints(SheetLine sheet)
    {
        if (sheet.line == null)
            return;
        sheet.line.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++)
            sheet.line.SetPosition(i, points[i]);
    }

    void NextFrame()
    {
        NextFrame(coreLine);
        NextFrame(glowLine);
    }

    void NextFrame(SheetLine sheet)
    {
        if (sheet.line == null)
            return;
        if (sheet.block == null)
            InitLine(sheet);

        int rows = Mathf.Max(1, sheet.rows);
        if (sheet.randomRow && rows > 1)
        {
            int next = Random.Range(0, rows - 1);
            sheet.row = next >= sheet.row ? next + 1 : next; // 같은 줄 연속 방지
        }
        else
        {
            sheet.row = (sheet.row + 1) % rows;
        }

        // 시트 맨 윗줄 = row 0 (UV v는 아래에서 위로 증가)
        float h = 1f / rows;
        float offsetY = 1f - h * (sheet.row + 1);
        bool flip = sheet.randomFlip && Random.value < 0.5f;
        Vector4 st = flip ? new Vector4(1f, -h, 0f, offsetY + h) : new Vector4(1f, h, 0f, offsetY);

        sheet.line.GetPropertyBlock(sheet.block);
        sheet.block.SetVector(MAIN_TEX_ST, st);
        sheet.line.SetPropertyBlock(sheet.block);

        float jitter = sheet.widthJitter > 0f ? Random.Range(1f - sheet.widthJitter, 1f + sheet.widthJitter) : 1f;
        sheet.line.widthMultiplier = sheet.width * jitter * widthScale;
    }

    void SetVisible(bool visible)
    {
        if (coreLine.line != null)
            coreLine.line.enabled = visible;
        if (glowLine.line != null)
            glowLine.line.enabled = visible;
        SetFxActive(startFx, visible);
        SetFxActive(endFx, visible);
    }

    static void SetFxActive(Transform fx, bool active)
    {
        if (fx != null && fx.gameObject.activeSelf != active)
            fx.gameObject.SetActive(active);
    }
}
