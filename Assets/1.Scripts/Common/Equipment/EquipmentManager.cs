using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
public class EquipmentManager : MonoSingleton<EquipmentManager>
{

    public EquipmentData[] equipmentDatas;
    public Dictionary<string, EquipmentData> equipmentDataDic = new Dictionary<string, EquipmentData>();
    public UniTask LoadTask { get; private set; }

    void Awake()
    {
        LoadTask = LoadDataAsync();
    }

    async UniTask LoadDataAsync()
    {
        await AddressableMgr.LoadAllByLabel<EquipmentData>("EquipmentData", (dates) =>
        {
            equipmentDatas = dates;
            foreach (EquipmentData equipmentData in equipmentDatas)
            {
                equipmentDataDic[equipmentData.key] = equipmentData;
            }

        });
     
    }

    public EquipmentData GetEquipmentData(string key)
    {
        return equipmentDataDic[key];
    }
    
}
