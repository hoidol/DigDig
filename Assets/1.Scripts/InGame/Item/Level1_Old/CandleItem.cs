using Unity.Cinemachine;
using UnityEngine;

public class CandleItem : Item
{
    public float[] sizeIncreases = {2.5f,3.5f};
    CinemachineCamera cinemachineCamera;

    public override void OnEquip()
    {
        cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        UpdateItem();
    }

    public override void UpdateItem()
    {
        if (cinemachineCamera == null) return;
        
        cinemachineCamera.Lens.OrthographicSize = CameraManager.INIT_ORTHOGRAPHIC_SIZE + sizeIncreases[count-1];
    }

    public override void OnUnequip()
    {
        if (cinemachineCamera == null)
            return;

        UpdateItem();
    }


    public override string GetDescription()
    {
        return $"시야가 넓어짐";
    }
}
