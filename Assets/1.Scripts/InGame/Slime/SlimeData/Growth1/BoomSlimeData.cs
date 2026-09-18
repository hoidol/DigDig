using UnityEngine;


[CreateAssetMenu(fileName ="Boom",menuName ="Growth1/Boom")]
public class BoomSlimeData : EnhanceAbilitySlimeData
{
    public float[] ranges = {2f,2.4f,2.8f};
    public int maxHitCount = 5;
    public override string GetDescription(int level =0)
    {
        return $"폭발하는 탄을 발사합니다(최대 5기)";
    }

}