using UnityEngine;

// 타이탄: 적을 처치할수록 크기와 공격력 증가
public class TitanBulletSpec : BulletSpec
{
    public static readonly float[] SIZE_PER_KILL   = { 0.15f, 0.18f, 0.22f };
    public static readonly float[] DAMAGE_PER_KILL = { 0.10f, 0.13f, 0.16f };
    public int killCount = 0;


    public TitanBulletSpec() { key = "Titan"; }
    

    

    // public override string GetDescription(int lv = 1, bool detail = false)
    // {
    //     return $"처치 시 크기 +{SIZE_PER_KILL[lv - 1] * 100:0}%, 데미지 +{DAMAGE_PER_KILL[lv - 1] * 100:0}% 증가";
    // }
}
