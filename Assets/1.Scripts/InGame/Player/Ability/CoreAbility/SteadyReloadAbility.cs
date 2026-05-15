using UnityEngine;

// 정교한 장전 - X초 이상 정지 시 장전 속도 증가
public class SteadyReloadAbility : Ability
{
    static readonly float[] waitTimes = { 5f, 4f, 3f, 2f, 1f };
    static readonly float[] speedUps  = { 0.70f, 0.75f, 0.80f, 0.90f, 1.00f };

    Buff buff;
    bool buffActive;
    float stillTimer;

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"{waitTimes[c - 1]}초 정지 시 장전 속도 {speedUps[c - 1] * 100:0}% 향상";
    }

    public override void OnUnequip(Player player)
    {
        RemoveBuff();
        stillTimer = 0;
    }

    public override void UpdateEnhancement()
    {
        if (!buffActive) return;
        RemoveBuff();
        ApplyBuff();
    }

    void Update()
    {
        bool stopped = Player.Instance.rg.linearVelocity.sqrMagnitude < 0.01f;
        if (stopped)
        {
            stillTimer += Time.deltaTime;
            if (!buffActive && stillTimer >= waitTimes[count - 1])
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
        buff = new Buff(StatType.ReloadTime, 1f / (1f + speedUps[count - 1]), StatOpType.Multiply);
        Player.Instance.AddBuff(buff);
        buffActive = true;
    }

    void RemoveBuff()
    {
        if (!buffActive) return;
        Player.Instance.RemoveBuff(buff);
        buffActive = false;
    }
}
