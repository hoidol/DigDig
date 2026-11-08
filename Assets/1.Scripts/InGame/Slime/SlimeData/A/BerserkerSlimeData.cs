using UnityEngine;


// [A] 버서커: 주변(detectRanges)에서 적/돌이 죽을 때마다 공속 중첩 (중첩당 attackSpeedPerStacks, 최대 maxStack)
// 마지막 처치 후 stackDuration 동안 처치가 없으면 중첩 전부 사라짐 → 적이 계속 죽는 자리에 둬야 유지
[CreateAssetMenu(fileName = "Berserker", menuName = "SlimeData/A/Berserker")]
public class BerserkerSlimeData : EnhanceAbilitySlimeData
{
    // mergeLv별 처치 감지 범위 (슬라임 위치 기준 반지름, 월드 단위)
    public float[] detectRanges = { 2f, 2.25f, 2.5f, 2.75f, 3f };
    // mergeLv별 중첩당 공속 증가 (0.05 = 5%, 합연산 후 곱)
    public float[] attackSpeedPerStacks = { 0.05f, 0.06f, 0.07f, 0.08f, 0.1f };
    // 최대 중첩 (강화로 늘어남)
    public int maxStack = 10;
    // 마지막 처치 후 중첩 유지 시간(초)
    public float stackDuration = 3f;

    public override string GetDescription(int mergeLv = 0)
    {
        int percent = Mathf.RoundToInt(attackSpeedPerStacks.GetByMergeLevel(mergeLv) * 100f);
        return string.Format(TranslateManager.GetText("Slime.Berserker.Desc"), detectRanges.GetByMergeLevel(mergeLv), percent, maxStack, stackDuration);
    }
}
