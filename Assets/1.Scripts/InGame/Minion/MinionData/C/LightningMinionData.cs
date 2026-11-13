using UnityEngine;


[CreateAssetMenu(fileName = "Lightning", menuName = "MinionData/C/Lightning")]
public class LightningMinionData : EnhanceMinionData
{

    public float searchRadius = 2f;
    public float initSearchRadius = 6f;

    public int[] lightningCounts = { 3, 4, 5, 6, 7 };


    public override string GetDescription(int level = 0)
    {
        return string.Format(TranslateManager.GetText("Minion.Lightning.Desc"), lightningCounts.GetByMergeLevel(level));
    }

}
