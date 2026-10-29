
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

    private void Start()
    {
        perlin = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        perlin.AmplitudeGain = 0f;
        AlignRightEdge();
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
