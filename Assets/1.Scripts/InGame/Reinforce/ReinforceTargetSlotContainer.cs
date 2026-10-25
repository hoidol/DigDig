using System;
using UnityEngine;

// 강화 대상 릴: 킹슬라임(캐릭터) 1 + 장착 슬라임 5 + 꽝 1 = 7칸
public class ReinforceTargetSlotContainer : ReinforceSlotContainer
{
    public Sprite noneThum; // 꽝 썸네일

    protected override string[] GetKeys()
    {
        string[] targets = new string[7];
        targets[0] = UserDataManager.Instance.userData.characterName;
        for (int i = 0; i < 5; i++)
        {
            targets[i + 1] = UserDataManager.Instance.userSlimeManager.userSlimeData.userSlimes[i].key;
        }
        targets[6] = ReinforceSlot.NONE_KEY; //꽝!
        return targets;
    }

    // 슬라임이면 SlimeData.thum, 캐릭터면 CharacterData.thum, 꽝이면 noneThum
    protected override Sprite GetThum(string key)
    {
        if (key == ReinforceSlot.NONE_KEY)
            return noneThum;

        if (SlimeManager.Instance.slimeDataDic.TryGetValue(key, out SlimeData slimeData))
            return slimeData.thum;

        if (Enum.TryParse(key, out CharacterName characterName))
            return CharacterManager.Instance.GetCharacterData(characterName).thum;

        Debug.LogWarning($"ReinforceTargetSlotContainer 알 수 없는 key {key}");
        return noneThum;
    }
}
