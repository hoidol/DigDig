using UnityEngine;


[CreateAssetMenu(fileName = "Flame", menuName = "SlimeData/C/Flame")]
public class FlameSlimeData : EnhanceSlimeData
{
    public float burnDuration = 4;
    public float[] burnDPSES = {1, 2, 3, 4, 5f};
    #if UNITY_EDITOR
    public override void LoadData()
    {
        base.LoadData();
        displayStatTypes.Add(StatType.BurnDPS);
        SetUniqueStat(StatType.BurnDPS, string.Join("/", burnDPSES));   
    }
    #endif

    public override string GetDescription(int mergeLv = 0)
    {
        return string.Format(TranslateManager.GetText("Slime.Flame.Desc"), burnDuration, burnDPSES[mergeLv]);
        //빙결탄을 발사합니다. {durations[mergeLv]}초 동안 빙결 상태를 부여합니다
    }
}
