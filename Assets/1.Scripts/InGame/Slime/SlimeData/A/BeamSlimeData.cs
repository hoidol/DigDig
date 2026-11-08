using UnityEngine;


// [A] 광선 링크: 필드에 광선 슬라임이 2마리 이상이면 모두 서로 레이저로 연결, 레이저에 닿은 적에게 0.25초마다 피해 (간격 고정: BeamLinkNetwork.TICK_INTERVAL)
// 혼자 있는 광선 슬라임은 기본 탄 공격
[CreateAssetMenu(fileName = "Beam", menuName = "SlimeData/A/Beam")]
public class BeamSlimeData : EnhanceAbilitySlimeData
{
    // mergeLv별 레이저 두께 (적중 판정 폭). 레이저 하나의 두께는 연결된 두 슬라임 값의 평균
    public float[] lineWidths = { 0.15f, 0.2f, 0.25f, 0.3f, 0.35f };
    // 레이저 피해 = 연결된 두 슬라임 공격력 평균 × 배율. 배율 = 1 - 감소율 × (전체 레이저 수 - 1), 최소 minLineDamageMul
    // (모두 연결이라 레이저 수가 n(n-1)/2로 빠르게 늘어남 → 레이저 하나의 피해를 줄여 총합 폭증을 막음)
    public float damageReductionPerLine = 0.1f; // 강화 11에서 완화
    public float minLineDamageMul = 0.3f;

    public override string GetDescription(int mergeLv = 0)
    {
        return string.Format(TranslateManager.GetText("Slime.Beam.Desc"), BeamLinkNetwork.TICK_INTERVAL);
    }
}
