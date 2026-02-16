using System.Linq;
using UnityEngine;
public class AbilityManager : MonoSingleton<AbilityManager>
{
    public AbilityData[] abilityDatas;

    private void Awake()
    {
        abilityDatas = Resources.LoadAll<AbilityData>("AbilityData");
    }

    public AbilityData GetAbilityData(string key)
    {
        return abilityDatas.Where(w => w.key == key).FirstOrDefault();
    }
}