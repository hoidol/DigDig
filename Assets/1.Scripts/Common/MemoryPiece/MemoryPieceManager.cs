using System.Linq;
using UnityEngine;

public class MemoryPieceManager : MonoSingleton<MemoryPieceManager> 
{
    MemoryPieceAbilityData[] memoryPieceAbilityDatas;

    void Awake()
    {
        
    }

    public MemoryPieceAbilityData GetMemoryPieceAbilityData(string key)
    {
        return memoryPieceAbilityDatas.Where(e=> e.key == key).FirstOrDefault();
    }
}

//마스터하면 다음것을 찍을 수 있음 - 3개