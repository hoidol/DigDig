using UnityEngine;

//미니미 2개 소환 및 {durations[lv-1]}동안 플레이어 + 플레이어 공격 속도 증가\n쿨타임 : {coolTimes[lv-1]}초, 발사 당 체력 -{itemData.consumeHp}
public class BoostItem : TriggerCycleItem 
{
    public BoostMiniMe boostMiniMePrefab;
    public BoostMiniMe[] boostMiniMes = new BoostMiniMe[2];

    public float consumeTime = 5;

    float[] coolTimes = {10,8,6};
    float[] attackSpeeds = {1.4f,1.6f,1.8f};
    float[] durations = {4f,5f,6f};
    Buff attackSpeedBuff;

    public override void OnEquip()
    {
        for(int i =0;i<boostMiniMes.Length; i++)
        {
            if(boostMiniMes[i] == null)
            {
                boostMiniMes[i] = Instantiate(boostMiniMePrefab);
                Vector2 pos = (Vector2)Player.Instance.transform.position + Random.insideUnitCircle.normalized;
                boostMiniMes[i].Spawn(pos);
            }
        }
        base.OnEquip();
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
         for(int i =0;i<boostMiniMes.Length; i++)
        {
            if(boostMiniMes[i] != null)
            { 
                Destroy(boostMiniMes[i].gameObject);
            }
            boostMiniMes[i]=null;
        }

        OnDeactivate();
    }

    public override void UpdateItem()
    { 
        base.UpdateItem();
        for(int i =0;i<boostMiniMes.Length; i++)
        {
            boostMiniMes[i]?.SetLevel(count); 
        }

        coolTime = coolTimes[count-1];
        activeTime = durations[count-1];
    }


    float timer;
    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= consumeTime)
        {
            Player.Instance.AddHp(-itemData.consumeHp);
        }
    }


     public override void OnActivate()
    {
        IsActive =true;
        attackSpeedBuff = new Buff( StatType.AttackSpeed,attackSpeeds[count-1], StatOpType.Multiply);
        Player.Instance.AddBuff(attackSpeedBuff);
        
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"미니미 2개 소환 및 {durations[lv-1]}동안 플레이어 + 플레이어 공격 속도 증가\n쿨타임 : {coolTimes[lv-1]}초, 발사 당 체력 -{itemData.consumeHp}";
    }

    public override void OnDeactivate()
    {
        if(attackSpeedBuff != null)
            Player.Instance.RemoveBuff(attackSpeedBuff);
            
        attackSpeedBuff = null;
        IsActive =false;
    }

    
    
}