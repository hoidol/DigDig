using UnityEngine;
using System.Linq;
[CreateAssetMenu(fileName = "SynergyAbilityData", menuName = "Ability/SynergyAbilityData", order = 0)]
public class SynergyAbilityData : AbilityData
{
    public SynergyType synergyType;

    public override bool Unlocked()
    {
        SynergyData s = Player.Instance.abilityInventory
        .currentSynergyDatas.FirstOrDefault(e => e.synergyType == synergyType);
        // Debug.Log("SynergyAbilityData Unlocked()");
        if (s == null)
        {
            // Debug.Log("SynergyAbilityData Unlocked()  if (s != null)");
            return false;
        }


        return base.Unlocked();

    }

}

