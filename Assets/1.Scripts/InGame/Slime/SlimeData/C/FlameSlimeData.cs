using UnityEngine;


[CreateAssetMenu(fileName = "Flame", menuName = "SlimeData/C/Flame")]
public class FlameSlimeData : EnhanceAbilitySlimeData
{
    public float burnDuration = 4;
    public float[] burnDPSES = {1, 2, 3, 4, 5f};
    
    public override void LoadData()
    {
        base.LoadData();
        displayStatTypes.Add(StatType.BurnDPS);
        SetUniqueStat(StatType.BurnDPS, string.Join("/", burnDPSES));   
    }

    public override string GetDescription(int mergeLv = 0)
    {
        return $"화염탄을 발사합니다. 화염 시 {burnDuration}초 동안 초당 {burnDPSES[mergeLv]} 피해를 줍니다.";
        //빙결탄을 발사합니다. {durations[mergeLv]}초 동안 빙결 상태를 부여합니다
    }
}
