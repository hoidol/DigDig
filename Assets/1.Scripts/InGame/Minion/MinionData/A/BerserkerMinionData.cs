using UnityEngine;


// [A] 버서커: 근접 공격, 공격 사거리(AttackRange) 안에서 적이 죽을 때마다 공속 중첩 (중첩당 attackSpeedPerStacks, 최대 maxStack)
// 마지막 처치 후 stackDuration 동안 처치가 없으면 중첩 전부 사라짐 → 적이 계속 죽는 자리에 둬야 유지
[CreateAssetMenu(fileName = "Berserker", menuName = "MinionData/A/Berserker")]
public class BerserkerMinionData : EnhanceMinionData
{
    // mergeLv별 중첩당 공속 증가 (0.05 = 5%, 합연산 후 곱)
    public float[] attackSpeedPerStacks = { 0.1f, 0.125f, 0.15f, 0.175f, 0.2f };
    // 최대 중첩 (강화로 늘어남)
    public int maxStack = 10;
    // 마지막 처치 후 중첩 유지 시간(초)
    public float stackDuration = 3f;

    public override string GetDescription(int mergeLv = 0)
    {
        int percent = Mathf.RoundToInt(attackSpeedPerStacks.GetByMergeLevel(mergeLv) * 100f);
        return string.Format(TranslateManager.GetText("Minion.Berserker.Desc"), GetBaseStat(StatType.AttackRange)?.GetValue<float>(mergeLv) ?? 0f, percent, maxStack, stackDuration);
    }
}
