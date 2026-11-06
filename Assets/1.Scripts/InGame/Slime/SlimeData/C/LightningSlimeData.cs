using UnityEngine;


[CreateAssetMenu(fileName = "Lightning", menuName = "SlimeData/C/Lightning")]
public class LightningSlimeData : EnhanceAbilitySlimeData
{

    public float searchRadius = 2f;
    public float initSearchRadius = 6f;

    public int[] lightningCounts = { 3, 4, 5, 6, 7 };


    public override string GetDescription(int level = 0)
    {
        return $"{lightningCounts.GetByMergeLevel(level)}기 적에게 체인 데미지를 입힙니다.";
    }

}
