using System.Linq;
using UnityEngine;

public class MemoryFragmentManager : MonoSingleton<MemoryFragmentManager> 
{
    MemoryFragmentAbilityData[] memoryFragmentAbilityDatas;

    void Awake()
    {
        
    }

    public MemoryFragmentAbilityData GetMemoryFragmentAbilityData(string key)
    {
        return memoryFragmentAbilityDatas.Where(e=> e.key == key).FirstOrDefault();
    }
}