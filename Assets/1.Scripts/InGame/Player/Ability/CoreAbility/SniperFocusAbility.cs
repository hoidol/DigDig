using UnityEngine;

// 예민함 유지 - X초 이상 정지 시 크리티컬 확률 증가
public class SniperFocusAbility : Ability
{
    static readonly float[] waitTimes = { 5f, 4f, 3f, 2f, 1f };
    static readonly float[] adds      = { 30f, 35f, 40f, 45f, 50f };

    Buff buff;
    bool buffApplied;
    float stillTimer;

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c == -1) c = count;
        if (c <= 0) c = 1;
        return $"{waitTimes[c - 1]}초 정지 시 크리티컬 확률 {adds[c - 1]:0}% 상승";
    }

    public override void OnUnequip(Player player)
    {
        RemoveBuff();
        stillTimer = 0;
    }

    public override void UpdateEnhancement()
    {
        if (!buffApplied) return;
        RemoveBuff();
        ApplyBuff();
    }

    void Update()
    {
        bool stopped = Player.Instance.rg.linearVelocity.sqrMagnitude < 0.01f;
        if (stopped)
        {
            stillTimer += Time.deltaTime;
            if (!buffApplied && stillTimer >= waitTimes[count - 1])
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
        buff = new Buff(StatType.CritChance, adds[count - 1], StatOpType.Add);
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
