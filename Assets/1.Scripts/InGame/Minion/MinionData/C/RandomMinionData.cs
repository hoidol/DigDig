using UnityEngine;


// 데미지 범위가 넓은 슬라임: AttackPower에 중간값만 넣고 attackPowerRandomRatio(예: 0.67)로 최소~최대 결정
[CreateAssetMenu(fileName ="Random",menuName = "MinionData/C/Random")]
public class RandomMinionData : EnhanceMinionData
{
    // public int[] shootCounts = {1,2,2,3,3,4};

    public override string GetDescription(int mergeLv =0)
    {
        float mid = GetMinionStat(StatType.AttackPower, 0).GetValue<float>(mergeLv);
        return string.Format(TranslateManager.GetText("Minion.Random.Desc"),
            Mathf.RoundToInt(mid * (1f - attackPowerRandomRatio)),
            Mathf.RoundToInt(mid * (1f + attackPowerRandomRatio)));
    }
}
