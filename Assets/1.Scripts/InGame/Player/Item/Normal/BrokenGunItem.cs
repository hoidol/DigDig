using UnityEngine;

// 고장난 총: (3초 > 2.5초 > 2초)마다 랜덤 방향으로 총 발사 (데미지 1배 > 1.5배 > 2배)
public class BrokenGunItem : TriggerItem
{
    static readonly float[] coolTimes = { 3f, 2.5f, 2f };
    static readonly float[] damageMultipliers = { 1f, 1.5f, 2f };

    public override void OnEquip(Player player)
    {
        UpdateItem();
        base.OnEquip(player);
    }

    public override void UpdateItem()
    {
        coolTime = coolTimes[UnityEngine.Mathf.Clamp(count - 1, 0, coolTimes.Length - 1)];
    }

    public override void OnTrigger()
    {
        base.OnTrigger();
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        float dmgMult = damageMultipliers[UnityEngine.Mathf.Clamp(count - 1, 0, damageMultipliers.Length - 1)];
        var dmgBuff = new Buff(StatType.AttackPower, dmgMult, StatOpType.Multiply);
        Player player = Player.Instance;
        player.AddBuff(dmgBuff);
        player.Shoot(dir, player.transform.position);
        player.RemoveBuff(dmgBuff);
    }
}
