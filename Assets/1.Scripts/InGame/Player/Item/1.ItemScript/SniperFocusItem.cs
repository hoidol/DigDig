using UnityEngine;

// 예민함 유지 - 2초 이상 정지 시 크리티컬 확률 40% 증가
public class SniperFocusItem : Item
{
    const float WAIT_TIME = 2f;
    const float CRIT_BONUS = 40f;

    Buff buff;
    bool buffApplied;
    float stillTimer;

    public override string GetDescription(bool detail = false)
    {
        return $"{WAIT_TIME}초 이상 이동하지 않으면 피해량 {CRIT_BONUS}% 증가";
    }

    public override void OnUnequip(Player player)
    {
        RemoveBuff();
        stillTimer = 0;
    }

    void Update()
    {
        bool stopped = Player.Instance.rg.linearVelocity.sqrMagnitude < 0.01f;
        if (stopped)
        {
            stillTimer += Time.deltaTime;
            if (!buffApplied && stillTimer >= WAIT_TIME)
                ApplyBuff();
        }
        else
        {
            stillTimer = 0;
            RemoveBuff();
        }
    }

    void ApplyBuff()
    {
        buff = new Buff(StatType.AttackPower, 1.2f, StatOpType.Multiply);
        Player.Instance.AddBuff(buff);
        buffApplied = true;
    }

    void RemoveBuff()
    {
        if (!buffApplied) return;
        Player.Instance.RemoveBuff(buff);
        buffApplied = false;
    }
}
