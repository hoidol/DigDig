using UnityEngine;


[CreateAssetMenu(fileName ="Pierce",menuName ="Growth1/Pierce")]
public class PierceSlimeData : EnhanceAbilitySlimeData
{
    public int[] pierceCount = {2,4,6};

    public override string GetDescription(int level =0)
    {
        return $"{pierceCount[level]}만큼 관통하는 탄을 발사합니다";
    }

}