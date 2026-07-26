using UnityEngine;

public class MiniMeItem : Item
{
    public MiniMe miniMePrefab;
    public MiniMe miniMe;
    public float consumeTime = 5;
    public float[] attackPowers = { 3, 6, 9 };
    public float[] attackSpeeds = { 2.5f, 2.5f, 2.5f };
    public override void OnEquip()
    {
        if (miniMe == null)
        {
            miniMe = Instantiate(miniMePrefab);
            Vector2 pos = (Vector2)Player.Instance.transform.position + Random.insideUnitCircle.normalized;
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
            miniMe.attackPower = attackPowers[count - 1];
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
        return $"미니미를 소환합니다. 미니 공격력 {attackPowers[lv - 1]}\n{consumeTime}초마다 체력 1 감소";
    }

}