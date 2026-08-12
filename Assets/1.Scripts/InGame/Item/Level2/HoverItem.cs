using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

//드론에 궤도가 달림, 이동 범위 자유로워짐
public class HoverItem : Item
{
    //자유롭게 돌아다님
    float orbitDamage = 2f;
    float attackPower = 6f;
    float attackSpeed = 2.5f;

    public HoverMiniMe miniMePrefab;
    public HoverMiniMe miniMe;
    public float consumeTime = 5;
    public override void OnEquip()
    {
        base.OnEquip();
        if (miniMe == null)
        {
            miniMe = Instantiate(miniMePrefab);
            Vector2 pos = (Vector2)Character.Instance.transform.position + Random.insideUnitCircle.normalized;
            miniMe.attackPower = attackPower;
            miniMe.attackSpeed = attackSpeed;
            miniMe.orbitDamage = orbitDamage;
            miniMe.Spawn(pos);
        }
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
            miniMe.attackPower = attackPower;
            miniMe.orbitDamage = orbitDamage;
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
        return $"궤도를 가진 비행 미니미를 소환합니다. 미니 공격력 {attackPower}\n{consumeTime}초마다 체력 {itemData.consumeHp} 감소";
    }
}