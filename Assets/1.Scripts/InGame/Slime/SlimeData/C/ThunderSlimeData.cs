using UnityEngine;


[CreateAssetMenu(fileName = "Thunder", menuName = "SlimeData/C/Thunder")]
public class ThunderSlimeData : EnhanceSlimeData
{
    // 머지 레벨별 낙뢰 범위(반지름). 머지할수록 넓어짐
    public float[] strikeRadiuses = { 1f, 1.2f, 1.4f, 1.6f, 1.8f };
    public int[] maxHitCounts = {5,6,7,8,9};

    public override string GetDescription(int mergeLv = 0)
    {
        return string.Format(TranslateManager.GetText("Slime.Thunder.Desc"), strikeRadiuses.GetByMergeLevel(mergeLv), maxHitCounts[mergeLv]);
    }
}
