using UnityEngine;

// [레킹볼]
// TriggerCycleItem. 활성화 시 WreckingBall을 소환해 플레이어 주변을 튕겨다니며 광석/적에게 피해.
// 활성 시간 10/15/20초, 데미지는 마력의 50%/70%/100%.
// 비활성화(OnDeactivate) 또는 장비 해제 시 볼 파괴.
public class WreckingBallItem : TriggerCycleItem
{
    public WreckingBall ballPrefab;

    WreckingBall ball;
    float[] activeTimes = { 10f, 15f, 20f };
    float[] damageRates = { 0.5f, 0.7f, 1 };

    public override void UpdateItem()
    {
        base.UpdateItem();
        activeTime = activeTimes[count - 1];
        if (ball == null) return;

        ball.Init(Player.Instance.statMgr.MagicPower, damageRates[count - 1]);
    }

    public override void OnActivate()
    {
        if (ball != null) return;
        ball = Instantiate(ballPrefab, Player.Instance.transform.position, UnityEngine.Quaternion.identity);
        ball.Init(Player.Instance.statMgr.MagicPower, damageRates[count - 1]);
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

    public override string GetDescription(int c = -1, bool detail = false)
    {
        return "활성화 시 도탄하며 광석과 적에게 피해를 줍니다.";
    }
}
