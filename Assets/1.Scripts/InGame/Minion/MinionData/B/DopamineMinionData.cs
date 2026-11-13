using UnityEngine;


// [B] 광란: 기본 탄 발사 + 주변(detectRanges)에서 적/돌이 죽으면 추가 탄 발사 (추가 탄은 공격력 ×extraShotDamageMuls)
[CreateAssetMenu(fileName = "Dopamine", menuName = "MinionData/B/Dopamine")]
public class DopamineMinionData : EnhanceMinionData
{
    // mergeLv별 처치 감지 범위 (슬라임 위치 기준 반지름, 월드 단위)
    public float[] detectRanges = { 2f, 2.25f, 2.5f, 2.75f, 3f };
    // mergeLv별 추가 탄 공격력 배율 (AttackPower에 곱함)
    public float[] extraShotDamageMuls = { 1.2f, 1.4f, 1.6f, 1.8f, 2f };
    // 추가 발사 수
    public int extraShotCount = 1;
    // 추가 발사 후 다음 추가 발사까지 쿨타임(초)
    public float extraShotCoolTime = 1f;

    public override string GetDescription(int mergeLv = 0)
    {
        int percent = Mathf.RoundToInt(extraShotDamageMuls.GetByMergeLevel(mergeLv) * 100f);
        return string.Format(TranslateManager.GetText("Minion.Dopamine.Desc"), detectRanges.GetByMergeLevel(mergeLv), percent, extraShotCount);
    }
}
