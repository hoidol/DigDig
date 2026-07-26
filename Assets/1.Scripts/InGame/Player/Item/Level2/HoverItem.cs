using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

//드론에 궤도가 달림, 이동 범위 자유로워짐
public class HoverItem : Item
{
    //자유롭게 돌아다님
    float[] orbitDamages = { 1f, 2f, 3f };
    float[] attackPowers = { 6, 11, 20 };
    float[] attackSpeeds = { 2.5f, 2.5f, 2.5f };

    public HoverMiniMe miniMePrefab;
    public HoverMiniMe miniMe;
    public float consumeTime = 5;
    public override void OnEquip()
    {
        base.OnEquip();
        if (miniMe == null)
        {
            miniMe = Instantiate(miniMePrefab);
            Vector2 pos = (Vector2)Player.Instance.transform.position + Random.insideUnitCircle.normalized;
            miniMe.attackPower = attackPowers[count - 1];
            miniMe.attackSpeed = attackSpeeds[count - 1];
            miniMe.orbitDamage = orbitDamages[count - 1];
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
            miniMe.attackPower = attackPowers[count - 1];
            miniMe.orbitDamage = orbitDamages[count - 1];
            miniMe.attackSpeed = attackSpeeds[count - 1];
        }
    }

    float timer;
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= consumeTime)
        {
            Player.Instance.AddHp(-itemData.consumeHp);
            timer = 0;
        }
    }

    public override string GetDescription(int lv = 1, bool detail = false)
    {
        return $"궤도를 가진 비행 미니미를 소환합니다. 미니 공격력 {attackPowers[lv - 1]}\n{consumeTime}초마다 체력 {itemData.consumeHp} 감소";
    }
}