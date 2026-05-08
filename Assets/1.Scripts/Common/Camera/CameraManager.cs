
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoSingleton<CameraManager>
{
    [SerializeField] private CinemachineCamera cinemachineCamera;

    private CinemachineBasicMultiChannelPerlin perlin;
    private float shakeTimer;

    private void Start()
    {
        perlin = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        perlin.AmplitudeGain = 0f;
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
