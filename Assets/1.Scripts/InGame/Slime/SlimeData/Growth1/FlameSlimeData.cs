using UnityEngine;


[CreateAssetMenu(fileName = "Flame", menuName = "Growth1/Flame")]
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
}
