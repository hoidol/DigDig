using UnityEngine;

// [레킹볼]
// TriggerCycleItem. 활성화 시 WreckingBall을 소환해 플레이어 주변을 튕겨다니며 광석/적에게 피해.
// 활성 시간 15초, 데미지는 마력의 70%.
public class WreckingBallItem : TriggerCycleItem
{
    const float ACTIVE_TIME = 15f;
    const float DAMAGE_RATE = 0.7f;

    public WreckingBall ballPrefab;

    WreckingBall ball;

    public override void OnEquip(Player player)
    {
        base.OnEquip(player);
        activeTime = ACTIVE_TIME;
    }

    public override void OnActivate()
    {
        if (ball != null) return;
        ball = Instantiate(ballPrefab, Player.Instance.transform.position, UnityEngine.Quaternion.identity);
        ball.Init(Player.Instance.statMgr.AttackPower, DAMAGE_RATE);
    }

    public override void OnDeactivate()
    {
        if (ball == null) return;
        Destroy(ball.gameObject);
        ball = null;
    }

    public override void OnUnequip(Player player)
    {
        base.OnUnequip(player);
        if (ball != null) { Destroy(ball.gameObject); ball = null; }
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return "활성화 시 도탄하며 광석과 적에게 피해를 줍니다.";
    }
}
