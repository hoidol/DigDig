
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoSingleton<CameraManager>
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    public Camera mainCamera;

    private CinemachineBasicMultiChannelPerlin perlin;
    private float shakeTimer;
    public const int INIT_ORTHOGRAPHIC_SIZE = 10;
    public Transform endRightTr;
    void Awake()
    {
        mainCamera= Camera.main;
    }

    #region 화면 드래그로 카메라 이동 (2026-11-24)
    // 빈 곳을 드래그하면 카메라가 움직임 (MinionFieldController). 범위 = 지금 맵(타일 + 경로) + panMargin
    // 맵이 화면 안에 다 들어오는 방향은 움직이지 않음 (처음 위치 그대로)
    public float panMargin = 0.5f;
    Vector3 homePos;
    bool homeSet;

    public Vector3 CameraPosition => cinemachineCamera != null ? cinemachineCamera.transform.position : Vector3.zero;

    // 화면 1픽셀 = 월드 거리
    public float WorldPerPixel => cinemachineCamera != null && Screen.height > 0 ? 2f * cinemachineCamera.Lens.OrthographicSize / Screen.height : 0f;

    public void PanTo(Vector3 desired)
    {
        if (cinemachineCamera == null)
            return;
        cinemachineCamera.transform.position = ClampToMap(desired);
    }

    Vector3 ClampToMap(Vector3 desired)
    {
        if (!homeSet)
            return desired;
        Stage stage = TileManager.Instance != null ? TileManager.Instance.ActiveStage : null;
        if (stage == null)
            return homePos;
        Bounds b = stage.GetWorldBounds();
        b.Expand(panMargin * 2f);
        float halfH = cinemachineCamera.Lens.OrthographicSize;
        float halfW = halfH * Screen.width / Mathf.Max(1, Screen.height);
        return new Vector3(
            ClampAxis(desired.x, homePos.x, halfW, b.min.x, b.max.x),
            ClampAxis(desired.y, homePos.y, halfH, b.min.y, b.max.y),
            homePos.z);
    }

    // 한 축: 맵이 화면보다 작거나 같으면 = 처음 위치(맵이 화면 밖으로 나가 있으면 들어오게만), 크면 = 화면이 맵 밖으로 안 나가게
    public static float ClampAxis(float desired, float home, float viewHalf, float min, float max)
    {
        if (max - min <= viewHalf * 2f)
            return Mathf.Clamp(home, max - viewHalf, min + viewHalf);
        return Mathf.Clamp(desired, min + viewHalf, max - viewHalf);
    }
    #endregion

    private void Start()
    {
        perlin = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        perlin.AmplitudeGain = 0f;
        AlignRightEdge();
        homePos = cinemachineCamera.transform.position;
        homeSet = true;
        PanTo(homePos); // 넓은 맵이면 범위 안으로
    }

    // 카메라 위치는 가운데 고정, 화면 비율에 맞춰 OrthographicSize만 조절 → 카메라 오른쪽 끝을 endRightTr에 맞춤
    public void AlignRightEdge()
    {
        if (endRightTr == null) return;

        float aspect = (float)Screen.width / Screen.height;
        float halfWidth = endRightTr.position.x - cinemachineCamera.transform.position.x;

        cinemachineCamera.Lens.OrthographicSize = halfWidth / aspect;
    }

    public void Shake(float intensity, float duration)
    {
        perlin.AmplitudeGain = intensity;
        shakeTimer = duration;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            //Debug.Log("")
            Shake(3f, 3f); // intensity, duration

        }
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0)
                perlin.AmplitudeGain = 0f;
        }
    }
}
