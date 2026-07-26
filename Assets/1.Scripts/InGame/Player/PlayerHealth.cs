using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class PlayerHealth : MonoBehaviour, IHittable
{

    public float MaxHp => player.statMgr.MaxHp;
    public float CurHp => curHp;
    public float curHp;
    public float healMultiplier = 1f;
    public PlayerHealthCanvas healthCanvas;

    Player player;
    StatusEffectHandler statusEffectHandler;
    Transform hpPoint;

    public Transform Transform => transform;

    public void Init(Player player, Transform hpPoint, StatusEffectHandler seh)
    {
        this.player = player;
        this.hpPoint = hpPoint;

        curHp = player.statMgr.MaxHp;
        RunRecover().Forget();
        statusEffectHandler = seh;
        healthCanvas.UpdateCanvas();
    }

    public void TakeDamage(DamageData damageData)
    {
        if (statusEffectHandler != null && statusEffectHandler.TryBlock()) return;
        PlayerTakeDamageText.SetText(hpPoint.position, $"-{(int)damageData.damage}");
        curHp -= damageData.damage;
        if (curHp <= 0)
            curHp = 0;

        GameEventBus.Publish(new PlayerHpChangedEvent(curHp, player.statMgr.MaxHp));
    }

    public void AddHp(float hp, bool showDmg = true)
    {
        Debug.Log($"PlayerHealth Add {hp}");
        if (hp > 0) hp *= healMultiplier;
        curHp += hp;
        if (showDmg)
        {
            if (hp > 0)
                HealText.SetText((Vector2)hpPoint.position + UnityEngine.Random.insideUnitCircle * 0.2f, ((int)hp).ToString(), GameSetting.healColor);
            else if (hp < 0)
                DamageText.SetText((Vector2)hpPoint.position + UnityEngine.Random.insideUnitCircle * 0.2f, $"{(int)hp}", GameSetting.enemyDamageColor);
        }


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
    public void ApplyStatusEffect(StatusEffect effect)
    {

    }
}
