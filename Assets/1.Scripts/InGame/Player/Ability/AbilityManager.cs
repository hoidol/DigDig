using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class AbilityManager : MonoSingleton<AbilityManager>
{
    public AbilityData[] abilityDatas;
    public SynergyData[] synergyDatas;

    private void Awake()
    {
        abilityDatas = Resources.LoadAll<AbilityData>("AbilityData");
        synergyDatas = Resources.LoadAll<SynergyData>("SynergyData");
    }

    public AbilityData GetAbilityData(string key)
    {
        return abilityDatas.Where(w => w.key == key).FirstOrDefault();
    }

    [SerializeField] List<AbilityData> canPickAbilityDatas = new List<AbilityData>();
    List<AbilityData> currentSynergyAbilityDatas = new List<AbilityData>();
    List<AbilityData> canPickSynergyAbilityDatas = new List<AbilityData>();
    public List<AbilityData> GetAbilityDatas(int count)
    {
        currentSynergyAbilityDatas.Clear();
        for (int i = 0; i < Player.Instance.abilityInventory.currentSynergyDatas.Count; i++)
        {
            foreach (var aRes in Player.Instance.abilityInventory.currentSynergyDatas[i].abilitieResources)
            {
                Ability a = Player.Instance.abilityInventory.GetAbility(aRes.key);
                if (a != null && a.count >= aRes.count)
                    continue;
                currentSynergyAbilityDatas.Add(GetAbilityData(aRes.key));
            }
        }
        canPickSynergyAbilityDatas.Clear();
        foreach (var sAbility in synergyDatas)
        {
            if (sAbility.CanPickSynergyAbility())
                canPickSynergyAbilityDatas.Add(GetAbilityData(sAbility.synergyAbilityKey));
        }


        canPickAbilityDatas.Clear();
        foreach (var abilityData in abilityDatas)
        {
            var playerAbility = Player.Instance.abilityInventory.equippedAbilitys.FirstOrDefault(a => a.key == abilityData.key);
            // Debug.Log($"abilityData key {abilityData.key}");
            if (!abilityData.Unlocked())
            {
                // Debug.Log($"abilityData locked");
                continue;
            }


            if (playerAbility != null && abilityData.maxLv > 0 && playerAbility.count >= abilityData.maxLv)
            {
                // Debug.Log($"abilityData max Count");
                continue;
            }


            canPickAbilityDatas.Add(abilityData);
        }

        // 가중치 풀 구성 - 시너지 최종 어빌리티 80, 시너지 연관 40, 기본 10
        var pool = new List<AbilityPickChance>();
        foreach (var data in canPickAbilityDatas)
        {
            float weight;
            if (canPickSynergyAbilityDatas.Contains(data))
                weight = 80f;
            else if (currentSynergyAbilityDatas.Contains(data))
                weight = 40f;
            else
                weight = 10f;
            pool.Add(new AbilityPickChance { abilityData = data, chance = weight });
        }

        // 가중치 기반 비복원 추출
        var result = new List<AbilityData>();
        int pickCount = Mathf.Min(count, pool.Count);
        for (int i = 0; i < pickCount; i++)
        {
            float total = 0f;
            for (int k = 0; k < pool.Count; k++) total += pool[k].chance;

            float roll = Random.Range(0f, total);
            float cumulative = 0f;
            for (int j = 0; j < pool.Count; j++)
            {
                cumulative += pool[j].chance;
                if (roll < cumulative)
                {
                    result.Add(pool[j].abilityData);
                    pool.RemoveAt(j);
                    break;
                }
            }
        }

        return result;

    }
    public struct AbilityPickChance
    {
        public AbilityData abilityData;
        public float chance;
    }


}