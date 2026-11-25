using UnityEngine;


// [B] 낙인: 적중 시 확률로 낙인 (중첩 없음, 받는 피해 +damageBonus, 모든 아군 공격에 적용)
[CreateAssetMenu(fileName = "Stigma", menuName = "MinionData/B/Stigma")]
public class StigmaMinionData : MinionData
{
    //낙인 확률 (머지 레벨별, 0~1)
    public float[] stigmaChances = {0.35f,0.45f,0.55f,0.65f,0.75f};
    public float damageBonus = 0.15f; // 낙인 걸린 적이 받는 피해 증가 (0.15 = 15%)

    public override string GetDescription(int mergeLv = 0)
    {
        return string.Format(TranslateManager.GetText("Minion.Stigma.Desc"), stigmaChances.GetByMergeLevel(mergeLv) * 100, damageBonus * 100);
    }
}
