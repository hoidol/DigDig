using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// 돌연변이 미니미 근처에서 공격 시 20% 확률로 체력 +1 회복 
// 행운	미니미
public class MutantItem : Item
{
    //자유롭게 돌아다님
    float healRange = 2f;
    float healChance = 0.2f;
    float attackPower = 3f;
    float attackSpeed = 2.5f;


    public MutantMiniMe miniMePrefab;
    public MutantMiniMe miniMe;
    public float consumeTime = 5;
    public override void OnEquip()
    {
        if (miniMe == null)
        {
            miniMe = Instantiate(miniMePrefab);
            Vector2 pos = (Vector2)Character.Instance.transform.position + Random.insideUnitCircle.normalized;
            miniMe.Spawn(pos);
        }
        base.OnEquip();
    }
    public override void OnUnequip()
    {
        base.OnUnequip();
        if (miniMe != null)
        {
            Destroy(miniMe.gameObject);
            miniMe = null;
        }
    }

    public override void UpdateItem()
    {
        base.UpdateItem();
        if (miniMe != null)
        {
            miniMe.SetLevel(count);
            miniMe.healRange = healRange;
            miniMe.healChance = healChance;
            miniMe.attackPower = attackPower;
            miniMe.attackSpeed = attackSpeed;
        }
    }

    float timer;
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= consumeTime)
        {
            Character.Instance.AddHp(-itemData.consumeHp);
            timer = 0;
        }
    }

    public override string GetDescription()
    {
        return $"돌연변이 미니미를 소환합니다. 근처에서 공격 시 {healChance * 100}% 체력 회복, 미니 공격력 {attackPower}\n{consumeTime}초마다 체력 {itemData.consumeHp} 감소";
    }
}