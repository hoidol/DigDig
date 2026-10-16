using UnityEngine;


[CreateAssetMenu(fileName ="Boom",menuName ="Growth1/Boom")]
public class BoomSlimeData : EnhanceAbilitySlimeData
{
    public float[] ranges = {1.5f,1.9f,2.3f,2.6f,2.9f,3.2f};
    public int maxHitCount = 5;
    public override string GetDescription(int level =0)
    {
        return $"폭발하는 탄을 발사합니다(최대 5기)";
    }

}