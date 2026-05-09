using UnityEngine;

// 골드 보상 - Underground 단계마다 골드 획득
public class DepthRewardAbility : Ability
{

    public override void OnEquip(Player player)
    {
        Player.Instance.AddGold(RewardGold(GameManager.Instance.ordealClearCount));
    }

    public int RewardGold(int count)
    {
        return (count + 1) * 5;
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = 1;

        return $"골드 (시련 클리어 횟수+1 * 5)획득";
    }
}
