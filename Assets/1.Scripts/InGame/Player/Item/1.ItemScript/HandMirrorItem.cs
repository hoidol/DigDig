using UnityEngine;
// [손거울]
// 발사된 총알에 튕김(BounceBehavior)을 추가하는 IBullet.
// count만큼 튕김 횟수가 증가하며, 총알이 벽이나 적에 맞으면 반사 방향으로 계속 진행.
public class HandMirrorItem : Item
{
    int[] bounceCounts = {3,4,5};

    public override void OnEquip()
    {
        Player.Instance.AddBounce(bounceCounts[count-1]);
    }
    public override void OnUnequip()
    {
        Player.Instance.AddBounce(-bounceCounts[count-1]);
    }

    public override string GetDescription(int lv = 1, bool detail = false)
    {
        return $"탄이 튕김 +{bounceCounts[lv-1]}";
    }
}
