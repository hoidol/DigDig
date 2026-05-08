using UnityEngine;

// [방랑탄]
// TriggerCycleItem. 활성화(OnActivate) 시 랜덤 방향으로 RicochetBolt를 발사.
// 볼트는 화면 경계에서 반사되며 적/광석을 관통. 활성 시간 10/15/20초, 데미지는 공격력의 50%/70%/100%.
public class RicochetBoltItem : TriggerCycleItem
{
    public RicochetBolt boltPrefab;
    public RicochetBolt ricochetBolt;

    float[] activeTimes = { 10f, 15f, 20f };
    float[] damageRates = { 0.5f, 0.7f, 1 };
    public override void UpdateItem()
    {
        base.UpdateItem();
        activeTime = activeTimes[count - 1];
        if (ricochetBolt == null) return;

        ricochetBolt.SetDamageRate(damageRates[count - 1]);
    }

    public override void OnActivate()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        var bolt = Instantiate(boltPrefab, Player.Instance.transform.position, Quaternion.identity);
        bolt.Init(Player.Instance.statMgr.AttackPower, dir);
    }

    public override void OnDeactivate() { }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        return "랜덤 방향으로 관통 투사체를 발사합니다.\n화면 끝에 닿으면 반사됩니다.";
    }
}
