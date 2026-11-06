using UnityEngine;


[CreateAssetMenu(fileName = "Thunder", menuName = "SlimeData/C/Thunder")]
public class ThunderSlimeData : EnhanceAbilitySlimeData
{
    // 머지 레벨별 낙뢰 범위(반지름). 머지할수록 넓어짐
    public float[] strikeRadiuses = { 1f, 1.2f, 1.4f, 1.6f, 1.8f };
    public int[] maxHitCounts = {5,6,7,8,9};

    public override string GetDescription(int mergeLv = 0)
    {
        return $"번개를 떨어뜨려 주변 적에게 피해를 줍니다. (범위 {strikeRadiuses.GetByMergeLevel(mergeLv)}, 최대 {maxHitCounts[mergeLv]}기)";
    }
}
