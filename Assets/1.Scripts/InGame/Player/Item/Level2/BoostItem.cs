using UnityEngine;

//미니미 2개 소환 및 처치 시 미니미 추가탄 발사 \n쿨타임 : {coolTimes[lv-1]}초, 발사 당 체력 -{itemData.consumeHp}
public class BoostItem : Item 
{
    public BoostMiniMe boostMiniMePrefab;
    public BoostMiniMe[] boostMiniMes = new BoostMiniMe[2];

    public float consumeTime = 5;

    float[] coolTimes = {2,2,2};

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
    }

    public override void UpdateItem()
    { 
        base.UpdateItem();
        for(int i =0;i<boostMiniMes.Length; i++)
        {
            boostMiniMes[i]?.SetLevel(count); 
            boostMiniMes[i].coolTime =coolTimes[count-1];
        }
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



    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"미니미 2개 소환, 처지 시 {count}개 추가 탄 발사\n쿨타임 : {coolTimes[lv-1]}초, {consumeTime}초 당 체력 -{itemData.consumeHp}";
    }


    
    
}