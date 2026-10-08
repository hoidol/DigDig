using UnityEngine;


[CreateAssetMenu(fileName ="Wacky",menuName ="Growth1/Wacky")]
public class WackySlimeData : EnhanceAbilitySlimeData
{

// 10 - 3 ~ 15
// 20 - 5 ~ 30
// 30 - 10 ~ 50
      public Vector2[] damages;


    public override SlimeStat GetSlimeStat(SlimeStatType statType, int enhanceLv)
    {
        SlimeEnhanceInfo slimeEnhanceInfo =  GetCommonSlimeEnhanceInfo(enhanceLv);
        if(statType == SlimeStatType.AttackPower)
        {
            
        }
        // if(statType == null)
        return slimeEnhanceInfo.GetSlimeStat(statType);
    }
    public override string GetDescription(int mergeLv =0)
    {
        return string.Format(TranslateManager.GetText($"WackySlime_Desc"), damages[mergeLv].x, damages[mergeLv].y);
    }
}
