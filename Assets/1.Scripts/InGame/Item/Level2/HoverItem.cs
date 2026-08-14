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
    public List<HoverMiniMe> miniMes = new();
    public float consumeTime = 5;

    public override void OnUnequip()
    {
        base.OnUnequip();
        foreach (HoverMiniMe miniMe in miniMes)
        {
            Destroy(miniMe.gameObject);
        }
        miniMes.Clear();
    }

    public override void UpdateItem()
    {
        base.UpdateItem();

        while (miniMes.Count < count)
        {
            HoverMiniMe miniMe = Instantiate(miniMePrefab);
            Vector2 pos = (Vector2)Character.Instance.transform.position + Random.insideUnitCircle.normalized;
            miniMe.Spawn(pos);
            miniMes.Add(miniMe);
        }
        while (miniMes.Count > count)
        {
            HoverMiniMe miniMe = miniMes[^1];
            miniMes.RemoveAt(miniMes.Count - 1);
            Destroy(miniMe.gameObject);
        }

        foreach (HoverMiniMe miniMe in miniMes)
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