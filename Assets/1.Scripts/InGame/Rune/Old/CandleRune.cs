using Unity.Cinemachine;
using UnityEngine;

public class CandleRune : Rune
{
    public float[] sizeIncreases = {2.5f,3.5f};
    CinemachineCamera cinemachineCamera;

    public override void OnEquip()
    {
        cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        UpdateRune();
    }

    public override void UpdateRune()
    {
        if (cinemachineCamera == null) return;
        
        cinemachineCamera.Lens.OrthographicSize = CameraManager.INIT_ORTHOGRAPHIC_SIZE + sizeIncreases[0];
    }

    public override void OnUnequip()
    {
        if (cinemachineCamera == null)
            return;

        UpdateRune();
    }


    public override string GetDescription()
    {
        return $"시야가 넓어짐";
    }
}
