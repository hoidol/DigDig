using System.Collections.Generic;
using UnityEngine;

public class MiniMeItem : Item
{
    public MiniMe miniMePrefab;
    public List<MiniMe> miniMes = new();

    public float consumeTime = 5;
    public float attackPower = 3;
    public float attackSpeed =  1.5f;

    public override void OnUnequip()
    {
        base.OnUnequip();
        foreach (MiniMe miniMe in miniMes)
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
            MiniMe miniMe = Instantiate(miniMePrefab);
            Vector2 pos = (Vector2)Character.Instance.transform.position + Random.insideUnitCircle.normalized;
            miniMe.Spawn(pos);
            miniMes.Add(miniMe);
        }
        while (miniMes.Count > count)
        {
            MiniMe miniMe = miniMes[^1];
            miniMes.RemoveAt(miniMes.Count - 1);
            Destroy(miniMe.gameObject);
        }

        foreach (MiniMe miniMe in miniMes)
        {
            miniMe.SetLevel(count);
            miniMe.attackPower = attackPower;
            miniMe.attackSpeed = attackSpeed;
        }
    }
    // float timer;
    // void Update()
    // {
    //     timer += Time.deltaTime;
    //     if (timer >= consumeTime)
    //     {
    //         Character.Instance.AddHp(-itemData.consumeHp);
    //         timer = 0;
    //     }
    // }

    public override string GetDescription()
    {
        return $"미니미를 소환합니다. 미니 공격력 {attackPower}\n{consumeTime}초마다 체력 1 감소";
        //return string.Format(TranslateManager.GetText($"{key}_Desc"),attackPower,consumeTime);
    }

}