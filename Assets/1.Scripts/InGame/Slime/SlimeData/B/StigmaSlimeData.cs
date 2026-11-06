using UnityEngine;


// [B] 낙인: 적중 시 확률로 낙인 (최대 3중첩, 중첩당 받는 피해 +5%, 모든 아군 공격에 적용)
[CreateAssetMenu(fileName = "Stigma", menuName = "SlimeData/B/Stigma")]
public class StigmaSlimeData : EnhanceAbilitySlimeData
{
    //낙인 확률 (머지 레벨별, 0~1)
    public float[] stigmaChances = {0.35f,0.45f,0.55f,0.65f,0.75f};
    public int maxStack = 3;
    public float damagePerStack = 0.05f; // 중첩당 받는 피해 증가 (0.05 = 5%)

    public override string GetDescription(int mergeLv = 0)
    {
        return $"적중 시 {stigmaChances.GetByMergeLevel(mergeLv) * 100:0}% 확률로 낙인을 새깁니다. 낙인 중첩당 받는 피해 {damagePerStack * 100:0}% 증가 (최대 {maxStack}중첩)";
    }
}
