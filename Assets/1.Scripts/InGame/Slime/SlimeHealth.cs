using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class SlimeHealth : MonoBehaviour, IHittable
{

    public float MaxHp => character.statMgr.MaxHp;
    public float CurHp => curHp;
    public float curHp;
    public float healMultiplier = 1f;
    public SlimeHealthCanvas healthCanvas;

    Slime character;
    public StatusEffectHandler statusEffectHandler;
    Transform hpPoint;

    public Transform Transform => transform;

    public void Init(Slime character, Transform hpPoint, StatusEffectHandler seh)
    {
        this.character = character;
        this.hpPoint = hpPoint;

        curHp = character.statMgr.MaxHp;
        // RunRecover().Forget();
        statusEffectHandler = seh;
        healthCanvas.UpdateCanvas();
    }

    public void TakeDamage(DamageData damageData)
    {
        if (statusEffectHandler != null && statusEffectHandler.TryBlock())
            return;

        // if(UnityEngine.Random.value <= character.statMgr.Dodge)
        // {
        //     //회피함
        //     CharacterTakeDamageText.SetText(hpPoint.position, "회피");
        //     return;
        // }

        float prevHp = curHp;
        curHp -= damageData.ApplyDamage(hpPoint.position);
        HeroUpgradeSystem.OnSlimeDamaged(prevHp - Mathf.Max(0f, curHp));
        GameResultStats.RecordSlimeDamage(prevHp - Mathf.Max(0f, curHp)); // (2026-11-27) 결과 화면: 받은 피해 // (2026-11-24) 용사 강화: 에너지 충전·정예병 분노
        if (curHp <= 0)
        {
            // (2026-11-24) 용사 강화 LastStand: 판당 1번 버팀
            float survive = HeroUpgradeSystem.TryLastStand(MaxHp);
            if (survive > 0f)
            {
                curHp = survive;
                EffectManager.PlayPopText((Vector2)hpPoint.position + Vector2.up * 0.4f, TranslateManager.GetText("HeroUpgrade_LastStand"), new Color(1f, 0.6f, 0.3f));
            }
            else
            {
                curHp = 0;
                character.animator.Play("Dead");
            }
        }
        HeroUpgradeSystem.CheckLowHp(curHp, MaxHp); // (2026-11-24) 용사 강화 LowHpFreeze

        GameEventBus.Publish(new SlimeHpChangedEvent(curHp, character.statMgr.MaxHp));
    }

    public void AddHp(float heal, bool showDmg = true)
    {
        if (heal > 0) heal *= healMultiplier;

        curHp += heal;

        if (showDmg)
        {
            if (heal > 0)
                HealText.SetText((Vector2)hpPoint.position + UnityEngine.Random.insideUnitCircle * 0.2f, $"+{(int)heal}", ColorSetting.healColor);
            else if (heal < 0)
                DamageText.SetText((Vector2)hpPoint.position + UnityEngine.Random.insideUnitCircle * 0.2f, $"{(int)heal}", ColorSetting.enemyDamageColor);
        }

        if (curHp > character.statMgr.MaxHp)
            curHp = character.statMgr.MaxHp;

        GameEventBus.Publish(new SlimeHpChangedEvent(curHp, character.statMgr.MaxHp));
    }

    public bool CanHit() => curHp > 0;

    // public async UniTaskVoid RunRecover()
    // {
    //     var token = this.GetCancellationTokenOnDestroy();
    //     while (!token.IsCancellationRequested)
    //     {
    //         // if (character.statMgr.RecoveryHp > 0)
    //         //     AddHp(character.statMgr.RecoveryHp);
    //         await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
    //     }
    // }
    public void ApplyStatusEffect(StatusEffect effect)
    {

    }
}
