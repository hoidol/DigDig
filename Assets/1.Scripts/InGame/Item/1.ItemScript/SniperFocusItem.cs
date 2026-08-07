using UnityEngine;

// 예민함 유지 - 2초 이상 정지 시 크리티컬 확률 25% 증가
public class SniperFocusItem : Item
{
    float[] WAIT_TIMES = {2f,1.5f,1f};
    float[] CRIT_BONUSES = {10,15,20};

    Buff buff;
    bool buffApplied;
    float stillTimer;

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"{WAIT_TIMES[lv-1]}초 이상 이동하지 않으면 크리티컬 확률 {CRIT_BONUSES[lv-1]}% 증가";
    }

    public override void OnUnequip()
    {
        RemoveBuff();
        stillTimer = 0;
    }

    void Update()
    {
        bool stopped = Character.Instance.rg.linearVelocity.sqrMagnitude < 0.01f;
        if (stopped)
        {
            stillTimer += Time.deltaTime;
            if (!buffApplied && stillTimer >= WAIT_TIMES[count-1])
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
        buff = new Buff(StatType.AttackPower, CRIT_BONUSES[count-1], StatOpType.Multiply);
        Character.Instance.AddBuff(buff);
        buffApplied = true;
    }

    void RemoveBuff()
    {
        if (!buffApplied) return;
        Character.Instance.RemoveBuff(buff);
        buffApplied = false;
    }
}
