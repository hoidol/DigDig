using UnityEngine;


[CreateAssetMenu(fileName ="Heal",menuName ="Growth1/Heal")]
public class HealSlimeData : EnhanceAbilitySlimeData
{    
    public float[] healCooltimes = {10,9,8};
    public override string GetDescription(int level =0)
    {
        return $"{healCooltimes[level]}초마다 캐릭터를 +{GetUniqueStat(SlimeStatType.Heal).GetValue<float>(level):0}회복시킴";
    }

}