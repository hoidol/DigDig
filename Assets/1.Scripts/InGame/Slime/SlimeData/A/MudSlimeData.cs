using UnityEngine;


// [A] 진흙: 공속마다 진흙 구슬 생성(회전), 닿으면 스플래시 피해 + 진흙 중첩 후 사라짐
// 진흙 중첩당 이동속도 15% 감소 (최대 80%, 엘리트·보스 60%). 멈추지는 않음 (MudEffect)
[CreateAssetMenu(fileName = "Mud", menuName = "SlimeData/A/Mud")]
public class MudSlimeData : EnhanceAbilitySlimeData
{
    // 머지 레벨별 최대 구슬 수
    public int[] maxOrbCounts = { 1, 2, 3, 4, 5 };
    // 머지 레벨별 스플래시 범위(반지름)
    public float splashRadius =0.5f;
    public int maxHitCount = 5;
    public float orbitRotationSpeed = 120;
    public float radius = 2;
    public float mudDuration = 3f;  // 마지막 적중 후 진흙 중첩 유지 시간

    public override string GetDescription(int mergeLv = 0)
    {
        return $"진흙 구슬(최대 {maxOrbCounts.GetByMergeLevel(mergeLv)}개)이 주변을 돌다 적에 닿으면 터져 주변에 진흙을 묻힙니다. 진흙 중첩당 이동속도 {MudEffect.SLOW_PER_STACK * 100:0}% 감소 (최대 {MudEffect.MAX_SLOW * 100:0}%, 엘리트·보스 {MudEffect.MAX_SLOW_BIG * 100:0}%)";
    }
}
