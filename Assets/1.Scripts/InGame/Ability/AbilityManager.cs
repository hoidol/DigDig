using System.Collections.Generic;
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

    List<AbilityData> canPickAbilityDatas = new List<AbilityData>();
    public AbilityData[] GetAbilityDatas()
    {
        canPickAbilityDatas.Clear();
        foreach (var abilityData in abilityDatas)
        {
            // 이미 플레이어가 가진 능력은 제외
            var playerAbility = Player.Instance.playerAbilities.FirstOrDefault(a => a.key == abilityData.key);

            // 플레이어가 해당 능력을 소유하고 있고, 레벨이 최대치에 도달했다면 뽑을 수 없음
            if (playerAbility != null && abilityData.Unlocked())
                continue;

            if (abilityData.maxLv > 0 && playerAbility.count >= abilityData.maxLv)
                continue;

            // 그렇지 않으면 추가
            canPickAbilityDatas.Add(abilityData);
        }
        // canPickAbilityDatas 섞어서 3개만 배열로 반환해줘

        // 리스트 셔플 (Fisher-Yates 알고리즘)
        for (int i = canPickAbilityDatas.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = canPickAbilityDatas[i];
            canPickAbilityDatas[i] = canPickAbilityDatas[j];
            canPickAbilityDatas[j] = temp;
        }

        int count = Mathf.Min(3, canPickAbilityDatas.Count);
        return canPickAbilityDatas.Take(count).ToArray();

    }


}