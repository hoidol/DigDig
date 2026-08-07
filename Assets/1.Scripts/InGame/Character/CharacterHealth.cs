using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class CharacterHealth : MonoBehaviour, IHittable
{

    public float MaxHp => character.statMgr.MaxHp;
    public float CurHp => curHp;
    public float curHp;
    public float healMultiplier = 1f;
    public CharacterHealthCanvas healthCanvas;

    Character character;
    StatusEffectHandler statusEffectHandler;
    Transform hpPoint;

    public Transform Transform => transform;

    public void Init(Character character, Transform hpPoint, StatusEffectHandler seh)
    {
        this.character = character;
        this.hpPoint = hpPoint;

        curHp = character.statMgr.MaxHp;
        RunRecover().Forget();
        statusEffectHandler = seh;
        healthCanvas.UpdateCanvas();
    }

    public void TakeDamage(DamageData damageData)
    {
        if (statusEffectHandler != null && statusEffectHandler.TryBlock()) return;
        CharacterTakeDamageText.SetText(hpPoint.position, $"-{(int)damageData.damage}");
        curHp -= damageData.damage;
        if (curHp <= 0)
            curHp = 0;

        GameEventBus.Publish(new CharacterHpChangedEvent(curHp, character.statMgr.MaxHp));
    }

    public void AddHp(float hp, bool showDmg = true)
    {
        // Debug.Log($"CharacterHealth Add {hp}");
        if (hp > 0) hp *= healMultiplier;
        curHp += hp;
        if (showDmg)
        {
            if (hp > 0)
                HealText.SetText((Vector2)hpPoint.position + UnityEngine.Random.insideUnitCircle * 0.2f, ((int)hp).ToString(), GameSetting.healColor);
            else if (hp < 0)
                DamageText.SetText((Vector2)hpPoint.position + UnityEngine.Random.insideUnitCircle * 0.2f, $"{(int)hp}", GameSetting.enemyDamageColor);
        }


        if (curHp > character.statMgr.MaxHp)
            curHp = character.statMgr.MaxHp;
        GameEventBus.Publish(new CharacterHpChangedEvent(curHp, character.statMgr.MaxHp));
    }

    public bool CanHit() => curHp > 0;

    public async UniTaskVoid RunRecover()
    {
        var token = this.GetCancellationTokenOnDestroy();
        while (!token.IsCancellationRequested)
        {
            if (character.statMgr.RecoveryHp > 0)
                AddHp(character.statMgr.RecoveryHp);
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
        }
    }
    public void ApplyStatusEffect(StatusEffect effect)
    {

    }
}
