using UnityEngine;

// 정교한 장전 - 2초 이상 정지 시 장전 속도 100% 빨라짐
public class SteadyReloadAbility : Ability
{
    const float WAIT_TIME = 2f;

    Buff buff;
    bool buffActive;
    float stillTimer;

    public override string GetDescription(bool detail = false)
    {
        return $"{WAIT_TIME}초 이상 이동하지 않으면 장전 속도 100% 빨라짐";
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
            if (!buffActive && stillTimer >= WAIT_TIME)
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
        // buff = new Buff(StatType.ReloadTime, 0.5f, StatOpType.Multiply);
        // Player.Instance.AddBuff(buff);
        // buffActive = true;
    }

    void RemoveBuff()
    {
        if (!buffActive) return;
        Player.Instance.RemoveBuff(buff);
        buffActive = false;
    }
}
