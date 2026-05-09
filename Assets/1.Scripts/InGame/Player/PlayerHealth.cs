using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class PlayerHealth : MonoBehaviour, IHittable
{
    public float curHp;
    public float healMultiplier = 1f;

    Player player;
    StatusEffectHandler statusEffectHandler;
    Transform hpPoint;

    public Transform Transform => transform;

    public void Init(Player player, Transform hpPoint, StatusEffectHandler seh)
    {
        this.player = player;
        this.hpPoint = hpPoint;
        statusEffectHandler = seh;
    }

    public void TakeDamage(DamageData damageData)
    {
        if (statusEffectHandler != null && statusEffectHandler.TryBlock()) return;
        PlayerTakeDamageText.SetText(hpPoint.position, $"-{(int)damageData.damage}");
        curHp -= damageData.damage;
        GameEventBus.Publish(new PlayerHpChangedEvent(curHp, player.statMgr.MaxHp));
        if (curHp < 0)
            GameManager.Instance.EndGame(false);
    }

    public void AddHp(float hp)
    {
        if (hp > 0) hp *= healMultiplier;
        curHp += hp;
        if (hp > 0)
            HealText.SetText(hpPoint.position, ((int)hp).ToString());
        if (curHp > player.statMgr.MaxHp)
            curHp = player.statMgr.MaxHp;
        GameEventBus.Publish(new PlayerHpChangedEvent(curHp, player.statMgr.MaxHp));
    }

    public bool CanHit() => curHp > 0;

    public async UniTaskVoid RunRecover()
    {
        var token = this.GetCancellationTokenOnDestroy();
        while (!token.IsCancellationRequested)
        {
            if (player.statMgr.RecoveryHp > 0)
                AddHp(player.statMgr.RecoveryHp);
            await UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: token);
        }
    }
}
