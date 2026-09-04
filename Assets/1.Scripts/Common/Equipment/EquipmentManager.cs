using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
public class EquipmentManager : MonoSingleton<EquipmentManager>
{

    public EquipmentData[] equipmentDatas;
    public Dictionary<string, EquipmentData> equipmentDataDic = new Dictionary<string, EquipmentData>();
    public Dictionary<GradeType, List<string>> gradeGroupEquipmentDic = new Dictionary<GradeType, List<string>>();
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
                if (!gradeGroupEquipmentDic.ContainsKey(equipmentData.grade))
                {
                    gradeGroupEquipmentDic.Add(equipmentData.grade, new List<string>());
                }
                gradeGroupEquipmentDic[equipmentData.grade].Add(equipmentData.key);

            }

        });

    }

    public EquipmentData GetEquipmentData(string key)
    {
        return equipmentDataDic[key];
    }

    public EquipmentAbility GetSumEquipmentAbility(StatType statType)
    {
        EquipmentAbility sumAbility = new EquipmentAbility();
        sumAbility.statType = statType;
        UserEquipment[] equippedEquipments = UserManager.Instance.userEquipmentManager.GetEquippedUserEquipments();
        for (int i = 0; i < equippedEquipments.Length; i++)
        {
            EquipmentAbility ability = equippedEquipments[i].equipmentData.GetEquipmentAbility(statType);
            sumAbility.AddAbility(ability);

        }
        return sumAbility;
    }

    public float GetTotalStatValue(CharacterData characterData, StatType statType)
    {
        float baseValue = characterData.GetCharacterStat(statType)?.value ?? 0f;
        return baseValue + GetSumEquipmentAbility(statType).value;
    }

}
