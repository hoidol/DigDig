using Unity.Cinemachine;
using UnityEngine;

public class CandleItem : Item
{
    public float sizeIncrease = 2f;
    const float BASE_SIZE = 8f;

    CinemachineCamera cinemachineCamera;

    public override void OnEquip(Player player)
    {
        cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        ApplySize();
    }

    public override void UpdateItem()
    {
        ApplySize();
    }

    public override void OnUnequip(Player player)
    {
        if (cinemachineCamera != null)
            cinemachineCamera.Lens.OrthographicSize = BASE_SIZE;
    }

    void ApplySize()
    {
        if (cinemachineCamera == null) return;
        cinemachineCamera.Lens.OrthographicSize = BASE_SIZE + sizeIncrease * count;
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"시야가 넓어짐";
    }
}
