using UnityEngine;

// 정교한 장전 - 장전 중 정지 상태이면 장전 시간 감소, 움직이면 즉시 해제
public class SteadyReloadAbility : Ability
{
    static readonly float[] reductions = { 0.20f, 0.30f, 0.40f };

    Buff buff;
    bool buffActive;

    void Update()
    {
        if (!Player.Instance.isReloading)
        {
            RemoveBuff();
            return;
        }

        bool standing = Player.Instance.rg.linearVelocity.magnitude < 0.1f;
        if (standing && !buffActive)
        {
            buff = new Buff(StatType.ReloadTime, 1f - reductions[count - 1], StatOpType.Multiply);
            Player.Instance.AddBuff(buff);
            buffActive = true;
        }
        else if (!standing && buffActive)
        {
            RemoveBuff();
        }
    }

    public override void OnUnequip(Player player)
    {
        RemoveBuff();
    }

    void RemoveBuff()
    {
        if (!buffActive) return;
        Player.Instance.RemoveBuff(buff);
        buffActive = false;
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"정지 중 장전 시간 {reductions[c - 1] * 100:0}% 감소";
    }
}
