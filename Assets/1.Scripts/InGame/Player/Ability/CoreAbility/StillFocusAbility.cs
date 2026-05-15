using UnityEngine;
// 집중 대응 - X초 이상 정지 시 공격 속도 증가
public class StillFocusAbility : Ability
{
    static readonly float[] waitTimes = { 5f, 4f, 3f, 2f, 1f };
    static readonly float[] multipliers = { 1.70f, 1.75f, 1.80f, 1.90f, 2.00f };

    Buff buff;
    bool buffApplied;
    float stillTimer;

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c == -1) c = count;
        if (c <= 0) c = 1;
        return $"{waitTimes[c - 1]}초 정지 시 공격속도 {(multipliers[c - 1] - 1f) * 100:0}% 상승";
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
        buff = new Buff(StatType.AttackSpeed, multipliers[count - 1], StatOpType.Multiply);
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
