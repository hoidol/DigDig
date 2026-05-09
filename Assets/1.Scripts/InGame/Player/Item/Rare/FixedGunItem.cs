using UnityEngine;

// [개조된 총]
// TriggerItem. 쿨타임(Inspector 설정)마다 플레이어 공격 방향(dirTr.right)으로
// count에 따라 4/6/8발의 PlayerBullet을 연속 발사. 데미지는 플레이어 공격력과 동일.
public class FixedGunItem : TriggerItem
{
    static readonly int[] shotCounts = { 4, 6, 8 };

    int ShotCount => shotCounts[Mathf.Clamp(count - 1, 0, shotCounts.Length - 1)];

    public override void OnEquip(Player player)
    {
        UpdateItem();
        base.OnEquip(player);
    }

    public override void UpdateItem()
    {
        // coolTime은 Inspector에서 설정
    }

    public override void OnTrigger()
    {
        base.OnTrigger();
        Vector2 dir = Player.Instance.weapon.dirTr.right;
        int count = ShotCount;

        for (int i = 0; i < count; i++)
        {
            var bullet = PlayerBullet.Instantiate();
            bullet.transform.position = Player.Instance.transform.position;
            bullet.Shoot(dir, Player.Instance.statMgr.AttackPower);
        }
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        int shots = shotCounts[Mathf.Clamp(c - 1, 0, shotCounts.Length - 1)];
        return $"공격 방향으로 {coolTime}초마다 {shots}발 추가 발사";
    }
}
