using UnityEngine;


[CreateAssetMenu(fileName = "Ice", menuName = "Growth1/Ice")]
public class IceSlimeData : EnhanceAbilitySlimeData
{

    public float[] durations = { 1.5f, 2f, 2.5f, 2.75f, 3f };

    public override string GetDescription(int mergeLv = 0)
    {
        return $"빙결탄을 발사합니다. {durations[mergeLv]}초 동안 빙결 상태를 부여합니다.";
    }

}