using UnityEngine;

// 고장난 총: 1초마다 랜덤 방향으로 총 발사 (데미지 100%)
public class BowItem : TriggerItem
{
    public override void OnEquip()
    {
        coolTime = 1.5f;
        base.OnEquip();
    }

    public override void OnTrigger()
    {
        base.OnTrigger();
        Debug.Log("BowItem OnTrigger()");
        for(int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Character.Instance.weapon.Shoot(new CharacterBulletSpec(), dir);    
        }
        
    }
}
