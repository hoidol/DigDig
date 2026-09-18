using UnityEngine;


[CreateAssetMenu(fileName ="Lightning",menuName ="Growth1/Lightning")]
public class LightningSlimeData : EnhanceAbilitySlimeData
{
    
    public float searchRadius = 2f;
    public float initSearchRadius = 6f;

    public int[] lightningCounts = {3,5,7};


    public override string GetDescription(int level =0)
    {
        return $"{lightningCounts[level]}기 적에게 체인 데미지를 입힙니다.";
    }

}
