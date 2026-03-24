using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
public abstract class ActiveItem : Item
{
    // dir 방향으로 추가 발사하거나 각도 조정 등
    public float activeTime;

    public float coolTime;
    float coolTimer;

    public float CoolTimer { get => coolTimer; set => coolTimer = value; }

    public bool active;
    public ActiveTimer activeTimer;
    public override void OnEquip(Player player)
    {
        base.OnEquip(player);
        activeTimer = new ActiveTimer();
        activeTimer.StartTimer(this);
    }

    public virtual void OnActive()
    {
        active = true;
    }
    public virtual void OnInactive()
    {
        active = false;
        activeTimer.StartTimer(this);
    }
}