using UnityEngine;


[CreateAssetMenu(fileName = "Ice", menuName = "MinionData/C/Ice")]
public class IceMinionData : EnhanceMinionData
{

    public float[] durations = { 1.5f, 2f, 2.5f, 2.75f, 3f };
    // 빙결 이동속도 배율 (머지 레벨별): 30%, 40%, 50%, 60%, 70% 감속. 중첩 없이 가장 강한 값 유지 (IceEffect)
    public float[] slowRate = { 0.7f, 0.6f, 0.5f, 0.4f, 0.3f };//30%,40%,50%,60%,70%

    public override string GetDescription(int mergeLv = 0)
    {
        return string.Format(TranslateManager.GetText("Minion.Ice.Desc"), durations.GetByMergeLevel(mergeLv), UnityEngine.Mathf.RoundToInt((1f - slowRate.GetByMergeLevel(mergeLv)) * 100f));
    }

}