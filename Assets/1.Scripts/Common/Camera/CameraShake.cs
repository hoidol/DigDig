using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class CameraShake : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource impulseSource;
    void Awake()
    {
        impulseSource = GetComponentInChildren<CinemachineImpulseSource>();
    }

    public void Shake(float intensity)
    {
        impulseSource.GenerateImpulse(intensity);
    }
}
