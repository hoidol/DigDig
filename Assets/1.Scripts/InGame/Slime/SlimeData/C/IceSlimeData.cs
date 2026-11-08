using UnityEngine;


[CreateAssetMenu(fileName = "Ice", menuName = "SlimeData/C/Ice")]
public class IceSlimeData : EnhanceAbilitySlimeData
{

    public float[] durations = { 1.5f, 2f, 2.5f, 2.75f, 3f };

    public override string GetDescription(int mergeLv = 0)
    {
        return string.Format(TranslateManager.GetText("Slime.Ice.Desc"), durations[mergeLv]);
    }

}