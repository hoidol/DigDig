using UnityEngine;


// 데미지 범위가 넓은 슬라임: AttackPower에 중간값만 넣고 attackPowerRandomRatio(예: 0.67)로 최소~최대 결정
[CreateAssetMenu(fileName ="Wacky",menuName ="Growth1/Wacky")]
public class WackySlimeData : EnhanceAbilitySlimeData
{
    // public int[] shootCounts = {1,2,2,3,3,4};

    public override string GetDescription(int mergeLv =0)
    {
        float mid = GetSlimeStat(SlimeStatType.AttackPower, 0).GetValue<float>(mergeLv);
        return string.Format(TranslateManager.GetText($"WackySlime_Desc"),
            Mathf.RoundToInt(mid * (1f - attackPowerRandomRatio)),
            Mathf.RoundToInt(mid * (1f + attackPowerRandomRatio)));
    }
}
