using UnityEngine;

// 고장난 총: 1초마다 랜덤 방향으로 총 발사 (데미지 100%)
public class BrokenGunItem : TriggerItem
{
    public override void OnEquip()
    {
        coolTime = 1f;
        base.OnEquip();
    }

    public override void OnTrigger()
    {
        base.OnTrigger();
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        // Player player = Player.Instance;
        // player.Shoot(dir, player.transform.position);
        Character.Instance.weapon.Shoot(new NormalBullet(), dir);
    }
}
