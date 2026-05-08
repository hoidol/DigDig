using UnityEngine;

// 골드 보상 - Underground 단계마다 골드 획득
public class DepthRewardAbility : Ability
{
    static readonly int[] goldAmounts = { 10, 20, 30 };

    public override void OnEquip(Player player)
    {
        Player.Instance.AddGold(goldAmounts[GameManager.Instance.underground - 1]);
    }


    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = 1;
        else c = GameManager.Instance.underground - 1;
        return $"골드 +{goldAmounts[c - 1]} 획득";
    }
}
